using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// C F-11 regression: the legacy generated wrapper caught EVERY exception of GetPC_MSG2280_2281 and answered it in the
// response header (HandleMessageException), so a sender always got a 200 response it could parse. Two failures were
// instead raised as HTTP 400 by the migration: the lock held by another request (legacy: InfException
// ConcurrencyErrorTryAgain, 1402474) and a message without an AgentRequest (legacy: a NullReferenceException on its
// first line).
[TestFixture]
public class MessageInBandFailureTests
{
    private const int ConcurrencyErrorTryAgainCode = 1402474;

    [Test]
    public async Task LockHeldByAnotherRequestIsAnsweredInBandWithConcurrencyErrorTryAgain()
    {
        var response = await Send(new CertificateOfOriginRequestMessageDto
        {
            AgentRequest = new CertificateOfOriginAgentRequestDto
            {
                RequestReasonCode = (int)ERequestReason.GetRequestStatus,
                CertificateId = "COO-1001",
            },
        }, lockAcquired: false);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Has.Count.EqualTo(1));
            Assert.That(response.Exceptions![0].ExceptionType, Is.EqualTo(ConcurrencyErrorTryAgainCode));
            Assert.That(response.ApplicationId, Is.Zero);
            Assert.That(response.Feedback.CertificateId, Is.Null);
        });
    }

    [Test]
    public async Task MessageWithoutAnAgentRequestIsAnsweredInBandWithAGeneralException()
    {
        var response = await Send(new CertificateOfOriginRequestMessageDto { AgentRequest = null! }, lockAcquired: true);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Has.Count.EqualTo(1));
            Assert.That(response.Exceptions![0].ExceptionType, Is.Zero, "GeneralException");
            Assert.That(response.ApplicationId, Is.Zero);
        });
    }

    // The lock service itself failing (legacy: inside the wrapper's try, so answered in-band too) and the config read
    // failing are the same family.
    [Test]
    public async Task LockServiceFailureIsAnsweredInBandWithAGeneralException()
    {
        var response = await Send(StatusQuery(), lockAcquired: true, lockThrows: true);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Has.Count.EqualTo(1));
            Assert.That(response.Exceptions![0].ExceptionType, Is.Zero, "GeneralException");
        });
    }

    [Test]
    public async Task ConfigReadFailureIsAnsweredInBandWithAGeneralException()
    {
        var response = await Send(StatusQuery(), lockAcquired: true, configThrows: true);

        Assert.That(response.Exceptions![0].ExceptionType, Is.Zero, "GeneralException");
    }

    // Legacy released the lock in its finally whatever the outcome, an unacquired lock state included.
    [TestCase(false)]
    [TestCase(true)]
    public async Task TheLockIsReleasedWhateverTheOutcome(bool lockAcquired)
    {
        var released = new List<string>();
        await Send(StatusQuery(), lockAcquired, released: released);

        Assert.That(released, Is.EqualTo(new[] { "COO-1001" }));
    }

    private static CertificateOfOriginRequestMessageDto StatusQuery()
    {
        return new CertificateOfOriginRequestMessageDto
        {
            AgentRequest = new CertificateOfOriginAgentRequestDto
            {
                RequestReasonCode = (int)ERequestReason.GetRequestStatus,
                CertificateId = "COO-1001",
            },
        };
    }

    private static async Task<CertificateOfOriginRequestFeedbackResponseDto> Send(
        CertificateOfOriginRequestMessageDto request, bool lockAcquired, bool lockThrows = false, bool configThrows = false, List<string>? released = null)
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

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(Fake<ICertificateOfOriginsDal>());
        services.AddSingleton(Fake<IRequestMetadata>());
        services.AddSingleton(lockUtil);
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), parametersUtil);
        var response = await bl.GetPC22802281CertificateOfOriginRequest(request);
        return response;
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
