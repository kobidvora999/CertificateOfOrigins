using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding F-08 (the related-entity half): legacy dereferenced `collateral.RelatedEntity.ID`, so a temporary collateral without its
// related entity failed with a NullReferenceException before anything was saved. The migration wrote `RelatedEntity?.Id ?? 0`, which
// turned such a collateral into a permanent one bound to entity 0. It is rejected up front with a message that names the field.
// (The legacy's second ChangeTempCollateralRequest argument, `true`, cannot be expressed in the proxy contract and is not covered here.)
[TestFixture]
public class ChangeTempCollateralTests
{
    [Test]
    public void ACollateralWithoutItsRelatedEntityIsRejectedAndNothingIsSent()
    {
        var sent = new List<List<ChangeTempCollateralRequestDto>>();

        var exception = Assert.ThrowsAsync<RestValidationException>(async () => await ChangeTempCollateral(
            sent,
            new CollateralRequestDto { CollateralRequestId = 1, RelatedEntity = new VirtualEntityDto { Id = 10 } },
            new CollateralRequestDto { CollateralRequestId = 2, RelatedEntity = null }));

        Assert.Multiple(() =>
        {
            Assert.That(DescribeOwnProperties(exception!), Does.Contain("RelatedEntity"), "the rejection names the field");
            Assert.That(sent, Is.Empty, "no collateral is converted when one of them cannot be bound");
        });
    }

    [Test]
    public async Task CollateralsWithTheirRelatedEntityAreBoundToIt()
    {
        var sent = new List<List<ChangeTempCollateralRequestDto>>();

        await ChangeTempCollateral(sent, new CollateralRequestDto { CollateralRequestId = 1, RelatedEntity = new VirtualEntityDto { Id = 10 } });

        Assert.Multiple(() =>
        {
            Assert.That(sent, Has.Count.EqualTo(1));
            Assert.That(sent[0].Select(c => (c.CollateralRequestId, c.RelatedEntityId, c.EntityExternalId)), Is.EqualTo(new[] { (1, 10, "10") }));
        });
    }

    private static async Task ChangeTempCollateral(List<List<ChangeTempCollateralRequestDto>> sent, params CollateralRequestDto[] collaterals)
    {
        var proxy = Fake<ICollateralProxy>((method, args) =>
        {
            if (method.Name == "ChangeTempCollateralRequest")
            {
                sent.Add((List<ChangeTempCollateralRequestDto>)args![0]!);
            }

            return null;
        });
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AuthenticationRequestBl>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(proxy);
        var bl = new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());

        try
        {
            await (Task)typeof(AuthenticationRequestBl)
                .GetMethod("ChangeTempCollateralRequest", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(bl, [collaterals.ToList()])!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }

    // RestValidationException carries the field and the message in its own members (not in Exception.Message), so they are read from there.
    private static string DescribeOwnProperties(Exception exception)
    {
        var values = exception.GetType().GetProperties()
            .Where(p => p.DeclaringType != typeof(Exception) && p.GetIndexParameters().Length == 0)
            .Select(p => System.Text.Json.JsonSerializer.Serialize(p.GetValue(exception)));
        return string.Join(" ", values);
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    private static object? DefaultReturn(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    public class InterfaceFake : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = (_, _) => null;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return Handler(targetMethod!, args);
        }
    }
}
