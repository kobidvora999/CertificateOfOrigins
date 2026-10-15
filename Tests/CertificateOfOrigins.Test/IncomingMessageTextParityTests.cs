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

// Parity findings C F-16 / F-17 / F-22 (message 2280):
//  F-16 the "cannot transmit a published / cancelled certificate" text rendered the request reason and the status by their
//       Hebrew table Names; the migration rendered the enum identifiers ("NewCertificate", "Published");
//  F-17 a whitespace-only certificateID is "no certificate id" (MustSendCertificateID), not a certificate that doesn't exist;
//  F-22 the feedback's query URL was formatted for every certificate, with an empty guid when the certificate had none.
[TestFixture]
public class IncomingMessageTextParityTests
{
    private const int MustSendCertificateIdCode = 14026318;
    private const string QueryUrlFormat = "https://query.example/{0}";

    [Test]
    public async Task TransmittingAPublishedCertificateNamesTheReasonAndStatusInHebrew()
    {
        var certificate = new CertificateOfOrigin
        {
            Id = 5,
            CertificateNumber = "COO-5",
            RequestReasonCode = (int)ERequestReason.NewCertificate,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
        };

        var bl = CreateBl(certificate, queryUrlFormat: null);
        var contextType = typeof(CertificateOfOriginsBl).GetNestedType("MessageValidationContext", BindingFlags.NonPublic)!;
        var context = Activator.CreateInstance(contextType)!;
        var method = typeof(CertificateOfOriginsBl).GetMethod("CheckIfCertificatePublishedOrCanceled", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)method.Invoke(bl, ["COO-5", context])!;

        var exceptions = (List<CertificateOfOriginExceptionDto>)contextType.GetProperty("Exceptions")!.GetValue(context)!;
        Assert.That(exceptions, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(exceptions[0].ExceptionDescription, Does.Contain("הוספת תעודה חדשה"), "the request reason by its table Name");
            Assert.That(exceptions[0].ExceptionDescription, Does.Contain("מאושרת לפרסום באינטרנט"), "the status by its table Name");
            Assert.That(exceptions[0].ExceptionDescription, Does.Not.Contain("NewCertificate").And.Not.Contain("Published"));
        });
    }

    [Test]
    public async Task AWhitespaceCertificateIdOnCancellationIsAMissingCertificateId()
    {
        var response = await Cancel(certificateId: "   ", guid: null, queryUrlFormat: null);

        Assert.That(response.Exceptions, Is.Not.Null);
        Assert.That(response.Exceptions!.Select(e => e.ExceptionType), Does.Contain(MustSendCertificateIdCode));
    }

    [Test]
    public async Task ACertificateWithoutAGuidStillGetsTheQueryUrlWithAnEmptyGuid()
    {
        var response = await Cancel(certificateId: "COO-1001", guid: null, queryUrlFormat: QueryUrlFormat);

        Assert.That(response.Feedback.QueryUrl, Is.EqualTo("https://query.example/"));
    }

    [Test]
    public async Task ACertificateWithAGuidGetsItInTheQueryUrl()
    {
        var guid = Guid.NewGuid();
        var response = await Cancel(certificateId: "COO-1001", guid: guid, queryUrlFormat: QueryUrlFormat);

        Assert.That(response.Feedback.QueryUrl, Is.EqualTo($"https://query.example/{guid}"));
    }

    private static async Task<CertificateOfOriginRequestFeedbackResponseDto> Cancel(string certificateId, Guid? guid, string? queryUrlFormat)
    {
        var certificate = new CertificateOfOrigin
        {
            Id = 321,
            CertificateNumber = "COO-1001",
            Title = "COO-1001",
            OrganizationUnitId = 1,
            CustomerId = 777,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
            Guid = guid,
        };

        var bl = CreateBl(certificate, queryUrlFormat);
        var response = await bl.GetPC22802281CertificateOfOriginRequest(new CertificateOfOriginRequestMessageDto
        {
            AgentRequest = new CertificateOfOriginAgentRequestDto
            {
                RequestReasonCode = (int)ERequestReason.CertificateCancellation,
                CertificateId = certificateId,
            },
        });
        return response;
    }

    private static CertificateOfOriginsBl CreateBl(CertificateOfOrigin certificate, string? queryUrlFormat)
    {
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) => method.Name switch
        {
            "GetLatestCertificateByNumberForFeedback" => Task.FromResult<CertificateOfOrigin?>(certificate),
            _ => null,
        });

        var parameters = Fake<IParametersUtil>((method, _) =>
            queryUrlFormat is not null && method.ReturnType == typeof(Task<string>) ? Task.FromResult(queryUrlFormat) : null);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null));
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));
        services.AddSingleton(Fake<IExportDealFileProxy>());
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), parameters);
        return bl;
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
