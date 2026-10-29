using System.Reflection;
using AutoMapper;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.MapperProfile;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// C F-11: the legacy generated wrapper caught EVERY exception of GetPC_MSG2280_2281 and answered it in the response header
// (HandleMessageException), so a sender always got a 200. Pure REST (alternative 2, 28.10.2026) answers with the HTTP status
// instead: the lock held by another request (legacy: ConcurrencyErrorTryAgain under 200) is a 409, and a failing lock
// service or config read is not swallowed (a 500). The lock itself is unchanged.
[TestFixture]
public class MessageInBandFailureTests
{
    private const int SenderId = 55;

    [Test]
    public void LockHeldByAnotherRequestIsAConflict()
    {
        Assert.ThrowsAsync<RestConflictException>(() => Cancel(false));
    }

    [Test]
    public void LockServiceFailureIsNotAnsweredInBand()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => Cancel(true, true));
    }

    [Test]
    public void ConfigReadFailureIsNotAnsweredInBand()
    {
        Assert.ThrowsAsync<InvalidOperationException>(() => Cancel(true, false, true));
    }

    // A lock this request took is released whatever the outcome. One it did NOT take is never released (legacy did call
    // SafeReleaseAsync with an unacquired state; that could free the lock another request holds, so it is not reproduced).
    [Test]
    public async Task AnAcquiredLockIsReleased()
    {
        var released = new List<string>();
        await Cancel(true, false, false, released);

        Assert.That(released, Is.EqualTo(new[] { "COO-1001" }));
    }

    [Test]
    public void ALockThisRequestDidNotTakeIsNeverReleased()
    {
        var released = new List<string>();
        Assert.ThrowsAsync<RestConflictException>(() => Cancel(false, false, false, released));

        Assert.That(released, Is.Empty);
    }

    // DELETE community/certificate-of-origins/COO-1001 — a cancellation takes the lock keyed by the certificate number.
    private static async Task Cancel(bool lockAcquired, bool lockThrows = false, bool configThrows = false, List<string>? released = null)
    {
        var lockState = Fake<ILockState>((method, _) => method.Name == "get_IsAcquired" ? lockAcquired : null);
        var lockUtil = Fake<ILockUtil>((method, args) =>
        {
            if (method.Name == "LockUntilAsync")
            {
                return lockThrows ? throw new InvalidOperationException("lock service down") : Task.FromResult(lockState);
            }

            if (method.Name == "SafeReleaseAsync")
            {
                released?.Add((string)args![0]!);
            }

            return null;
        });
        var parametersUtil = Fake<IParametersUtil>((method, _) =>
        {
            if (method.Name == "Get" && method.ReturnType == typeof(Task<bool>))
            {
                return configThrows ? throw new InvalidOperationException("config down") : Task.FromResult(true);
            }

            return null;
        });

        var certificate = new CertificateOfOrigin
        {
            Id = 321,
            CertificateNumber = "COO-1001",
            Title = "COO-1001",
            CreateCustomerId = SenderId,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
        };
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) =>
            method.Name == "GetLatestCertificateByNumberForFeedback" ? Task.FromResult<CertificateOfOrigin?>(certificate) : null);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name switch
        {
            "get_UserId" => (int?)7,
            "get_MessageSenderId" => (int?)SenderId,
            _ => null,
        }));
        services.AddSingleton(lockUtil);
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));
        services.AddSingleton(Fake<IExportDealFileProxy>());
        services.AddSingleton<IMapper>(new MapperConfiguration(cfg => cfg.AddProfile<PureRestContractProfile>(), NullLoggerFactory.Instance).CreateMapper());
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), parametersUtil);
        await bl.CancelCertificateOfOriginByMessage("COO-1001");
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // The event builder's type is known only at runtime; every fluent call returns the same proxy.
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
