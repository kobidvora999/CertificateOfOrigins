using System.Reflection;
using AutoMapper;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.MapperProfile;
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
using CertificateOfOriginResult = Api.Models.CertificateOfOriginsCommunity.CertificateOfOriginResult;

namespace CertificateOfOrigins.Test;

// Parity findings C F-16 / F-17 / F-22 (message 2280):
//  F-16 the "cannot transmit a published / cancelled certificate" text rendered the request reason and the status by their
//       Hebrew table Names; the migration rendered the enum identifiers ("NewCertificate", "Published");
//  F-17 a whitespace-only certificateID is "no certificate id" - gone in pure REST: the certificate number is a URL segment;
//  F-22 the feedback's query URL was formatted for every certificate, with an empty guid when the certificate had none.
[TestFixture]
public class IncomingMessageTextParityTests
{
    private const int SenderId = 55;
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
    public async Task ACertificateWithoutAGuidStillGetsTheQueryUrlWithAnEmptyGuid()
    {
        var result = await Status(null, QueryUrlFormat);

        Assert.That(result.QueryUrl, Is.EqualTo("https://query.example/"));
    }

    [Test]
    public async Task ACertificateWithAGuidGetsItInTheQueryUrl()
    {
        var guid = Guid.NewGuid();
        var result = await Status(guid, QueryUrlFormat);

        Assert.That(result.QueryUrl, Is.EqualTo($"https://query.example/{guid}"));
    }

    private static async Task<CertificateOfOriginResult> Status(Guid? guid, string? queryUrlFormat)
    {
        var certificate = new CertificateOfOrigin
        {
            Id = 321,
            CertificateNumber = "COO-1001",
            Title = "COO-1001",
            OrganizationUnitId = 1,
            CustomerId = 777,
            CreateCustomerId = SenderId,
            CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published,
            Guid = guid,
        };

        var bl = CreateBl(certificate, queryUrlFormat);
        var result = await bl.GetCertificateOfOriginStatusForMessage("COO-1001");
        return result;
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
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name switch
        {
            "get_UserId" => (int?)7,
            "get_MessageSenderId" => (int?)SenderId,
            _ => null,
        }));
        services.AddSingleton<IMapper>(new MapperConfiguration(cfg => cfg.AddProfile<PureRestContractProfile>(), NullLoggerFactory.Instance).CreateMapper());
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
