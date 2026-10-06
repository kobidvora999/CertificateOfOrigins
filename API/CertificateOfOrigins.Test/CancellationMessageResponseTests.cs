using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// C F-10 regression: in legacy the exception gate (throw _requestExceptions) ran before the reason switch, and the wrapper
// answered a throw with a FRESH response - empty feedback, ApplicationId 0, only the header exceptions. A cancellation
// that was blocked (an open customs-employee task, a still-linked declaration) therefore never echoed the certificate it
// had not cancelled; the migration returned the full feedback and the certificate id next to the exceptions.
[TestFixture]
public class CancellationMessageResponseTests
{
    private const int CertificateId = 321;

    [Test]
    public async Task BlockedCancellationReturnsAnEmptyFeedbackAndNoApplicationId()
    {
        // A certificate with an open customs-employee task cannot be cancelled by message.
        var response = await Cancel((int)ECertificateOfOriginStatus.DeclarationMatch, linkedDeclaration: false);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Is.Not.Null.And.Not.Empty, "the block is reported in the header exceptions");
            Assert.That(response.ApplicationId, Is.Zero, "a blocked cancellation must not look like a success");
            Assert.That(response.Feedback.CertificateId, Is.Null, "the feedback of the un-cancelled certificate is not echoed");
        });
    }

    [Test]
    public async Task CancellationBlockedByALinkedDeclarationReturnsAnEmptyFeedbackAndNoApplicationId()
    {
        var response = await Cancel((int)ECertificateOfOriginStatus.Published, linkedDeclaration: true);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Is.Not.Null.And.Not.Empty);
            Assert.That(response.ApplicationId, Is.Zero);
            Assert.That(response.Feedback.CertificateId, Is.Null);
        });
    }

    [Test]
    public async Task CleanCancellationStillReturnsTheCancelledCertificate()
    {
        var response = await Cancel((int)ECertificateOfOriginStatus.Published, linkedDeclaration: false);

        Assert.Multiple(() =>
        {
            Assert.That(response.Exceptions, Is.Null);
            Assert.That(response.ApplicationId, Is.EqualTo(CertificateId));
            Assert.That(response.Feedback.CertificateId, Is.EqualTo("COO-1001"));
            Assert.That(response.Feedback.CertificateOfOriginStatusCode, Is.EqualTo((int)ECertificateOfOriginStatus.Cancelled));
        });
    }

    private static async Task<CertificateOfOriginRequestFeedbackResponseDto> Cancel(int storedStatusId, bool linkedDeclaration)
    {
        var certificate = new CertificateOfOrigin
        {
            Id = CertificateId,
            CertificateNumber = "COO-1001",
            Title = "COO-1001",
            OrganizationUnitId = 1,
            CustomerId = 777,
            CertificateOfOriginStatusId = storedStatusId,
        };

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) => method.Name switch
        {
            "GetLatestCertificateByNumberForFeedback" => Task.FromResult<CertificateOfOrigin?>(certificate),
            _ => null,
        });

        var exportDealFile = Fake<IExportDealFileProxy>((method, _) =>
            method.Name == "GetLeadDocumentByCertificateOfOriginId" && linkedDeclaration
                ? Task.FromResult<LeadDocumentByCertificateOfOriginDto?>(new LeadDocumentByCertificateOfOriginDto { LeadDocumentTitle = "DECL-1" })
                : null);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null));
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));
        services.AddSingleton(exportDealFile);
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), Fake<IParametersUtil>());

        var response = await bl.GetPC22802281CertificateOfOriginRequest(new CertificateOfOriginRequestMessageDto
        {
            AgentRequest = new CertificateOfOriginAgentRequestDto
            {
                RequestReasonCode = (int)ERequestReason.CertificateCancellation,
                CertificateId = "COO-1001",
            },
        });
        return response;
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
