using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
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

// Parity finding F-10: legacy wrote every row through one unit of work committed together, so a failed save left nothing behind. The
// migration's set-based writes commit one by one, and the 404 for a missing file / request surfaced only at the last step - after the
// children were updated, the collaterals made permanent, and the events and messages sent. A missing row is now a 404 before any of it.
[TestFixture]
public class MissingRowBeforeSideEffectsTests
{
    [Test]
    public void SavingAMissingFileIsA404BeforeAnythingIsWrittenOrRaised()
    {
        var effects = new List<string>();
        var bl = Bl(effects, fileExists: false, requestExists: true);

        Assert.ThrowsAsync<RestNotFoundException>(async () => await bl.SaveAuthenticationRequestFile(new SaveAuthenticationRequestFileRequestDto
        {
            Id = 11,
            Requests = [new SaveAuthenticationRequestFileChildDto { DocumentId = 1, DecisionId = 2, OriginalRequestDecisionId = 1 }],
        }));

        Assert.That(effects, Is.Empty, "no child row written, no event raised, no message sent");
    }

    [Test]
    public void UpdatingAMissingRequestIsA404BeforeTheCollateralsAndEvents()
    {
        var effects = new List<string>();
        var bl = Bl(effects, fileExists: true, requestExists: false);

        Assert.ThrowsAsync<RestNotFoundException>(async () => await bl.SaveImportAuthenticationRequest(new SaveImportAuthenticationRequestRequestDto
        {
            DocumentId = 104,
            IsNewInstance = false,
            Collaterals = [new CollateralRequestDto { CollateralRequestId = 1, RelatedEntity = new VirtualEntityDto { Id = 104 } }],
        }));

        Assert.That(effects, Is.Empty, "the collaterals are not made permanent and no event is raised");
    }

    private static AuthenticationRequestBl Bl(List<string> effects, bool fileExists, bool requestExists)
    {
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) =>
        {
            switch (method.Name)
            {
                case "GetAuthenticationFileById":
                    return Task.FromResult<CertificateOfOriginsImportAuthenticationFileDetails?>(fileExists ? new CertificateOfOriginsImportAuthenticationFileDetails() : null);
                case "GetImportAuthenticationRequestById":
                    return Task.FromResult<CertificateOfOriginsImportAuthenticationRequest?>(requestExists ? new CertificateOfOriginsImportAuthenticationRequest() : null);
                case "UpdateFileChildRequest":
                    effects.Add("UpdateFileChildRequest");
                    return Task.FromResult(true);
                default:
                    return null;
            }
        });
        var collateralProxy = Fake<ICollateralProxy>((method, _) =>
        {
            effects.Add("Collateral." + method.Name);
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
        services.AddSingleton(collateralProxy);
        services.AddSingleton(eventUtil);
        services.AddSingleton(Fake<ITasksProxy>());
        services.AddSingleton(Fake<IMessageManagementProxy>());
        return new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());
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
