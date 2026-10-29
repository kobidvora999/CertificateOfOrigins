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
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CertificateOfOriginResult = Api.Models.CertificateOfOriginsCommunity.CertificateOfOriginResult;

namespace CertificateOfOrigins.Test;

// C F-10 regression: in legacy the exception gate (throw _requestExceptions) ran before the reason switch, so a cancellation
// that was blocked (an open customs-employee task, a still-linked declaration) never echoed the certificate it had not
// cancelled. Pure REST (DELETE community/certificate-of-origins/{certificateId}): a blocked cancellation is a 400 with the
// legacy exceptions, never a response that could read as a success.
[TestFixture]
public class CancellationMessageResponseTests
{
    private const int CertificateId = 321;
    private const int SenderId = 55;

    [Test]
    public void BlockedCancellationIsABadRequest()
    {
        // A certificate with an open customs-employee task cannot be cancelled by message.
        var exception = Assert.ThrowsAsync<RestValidationException>(() => Cancel((int)ECertificateOfOriginStatus.DeclarationMatch, false, SenderId));

        Assert.That(exception!.Errors, Is.Not.Empty, "the block is reported as errors[]");
    }

    [Test]
    public void CancellationBlockedByALinkedDeclarationIsABadRequest()
    {
        var exception = Assert.ThrowsAsync<RestValidationException>(() => Cancel((int)ECertificateOfOriginStatus.Published, true, SenderId));

        Assert.That(exception!.Errors, Is.Not.Empty);
    }

    [Test]
    public async Task CleanCancellationReturnsTheCancelledCertificate()
    {
        var result = await Cancel((int)ECertificateOfOriginStatus.Published, false, SenderId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Exception, Is.Null);
            Assert.That(result.ApplicationId, Is.EqualTo(CertificateId));
            Assert.That(result.CertificateId, Is.EqualTo("COO-1001"));
            Assert.That(result.CertificateOfOriginStatusCode, Is.EqualTo((int)ECertificateOfOriginStatus.Cancelled));
        });
    }

    // New vs legacy (29.10.2026): a certificate another agent created is not found, and is not cancelled.
    [Test]
    public void AnotherSendersCertificateIsNotFound()
    {
        Assert.ThrowsAsync<RestNotFoundException>(() => Cancel((int)ECertificateOfOriginStatus.Published, false, SenderId + 1));
    }

    private static async Task<CertificateOfOriginResult> Cancel(int storedStatusId, bool linkedDeclaration, int messageSenderId)
    {
        var certificate = new CertificateOfOrigin
        {
            Id = CertificateId,
            CertificateNumber = "COO-1001",
            Title = "COO-1001",
            OrganizationUnitId = 1,
            CustomerId = 777,
            CreateCustomerId = SenderId,
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
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name switch
        {
            "get_UserId" => (int?)7,
            "get_MessageSenderId" => (int?)messageSenderId,
            _ => null,
        }));
        services.AddSingleton<IMapper>(new MapperConfiguration(cfg => cfg.AddProfile<PureRestContractProfile>(), NullLoggerFactory.Instance).CreateMapper());
        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));
        services.AddSingleton(exportDealFile);
        var serviceProvider = services.BuildServiceProvider();

        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), Fake<IParametersUtil>());

        var result = await bl.CancelCertificateOfOriginByMessage("COO-1001");
        return result;
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
