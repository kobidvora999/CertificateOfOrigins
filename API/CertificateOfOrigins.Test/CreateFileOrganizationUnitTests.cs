using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding F-11, first point: the legacy CreateNewAuthenticationFile read `OrganizationUnitIDNum.Value` for the file's unit. It threw when the
// first request carried none - after the events were queued but before the file was saved or anything committed, so nothing happened at all. The
// migration's `?? 0` inserted the file and raised its event with no unit. It is rejected up front, before any event or write. A unit of 0 is rejected
// as well (stricter than the legacy): the .NET 10 event builder refuses WithOrganizationUnitId(0), so it would fail only after the file was inserted.
[TestFixture]
public class CreateFileOrganizationUnitTests
{
    [TestCase(null)]
    [TestCase(0)]
    public void AFirstRequestWithoutARealUnitIsRejectedBeforeAnyEventOrWrite(int? unit)
    {
        var effects = new List<string>();
        var bl = Bl(effects);

        var exception = Assert.ThrowsAsync<RestValidationException>(async () => await bl.CreateNewAuthenticationFile(
            [new GetImportAuthenticationRequestResultDto { DocumentId = 104, OrganizationUnitIdNum = unit }]));

        Assert.Multiple(() =>
        {
            Assert.That(DescribeOwnProperties(exception!), Does.Contain("OrganizationUnitIdNum"), "the rejection names the field");
            Assert.That(effects, Is.Empty, "no event raised, no file written, no request linked");
        });
    }

    private static AuthenticationRequestBl Bl(List<string> effects)
    {
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) =>
        {
            if (method.Name is "Add" or "SaveChangesAsync" or "LinkRequestsToAuthenticationFile")
            {
                effects.Add("Dal." + method.Name);
            }

            return null;
        });
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var eventUtil = Fake<IEventUtil>((method, _) =>
        {
            effects.Add("Event." + method.Name);
            return method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null;
        });

        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<AuthenticationRequestBl>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null));
        services.AddSingleton(eventUtil);
        return new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());
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

    private static object FakeChainingBuilder(Type builderType)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, _) => method.ReturnType == builderType ? proxy : DefaultReturn(method);
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

        if (returnType == typeof(ValueTask))
        {
            return new ValueTask();
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(inner)
                .Invoke(null, [inner.IsValueType ? Activator.CreateInstance(inner) : null])!;
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
