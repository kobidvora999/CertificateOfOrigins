using System.Reflection;
using System.Xml.Linq;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Customers;
using IssueCertificateOfOrigin.Worker;
using IssueCertificateOfOrigin.Worker.Dal;
using IssueCertificateOfOrigin.Worker.Dto;
using IssueCertificateOfOrigin.Worker.Proxies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// The 2281 feedback the IssueCertificateOfOrigin worker sends, against the legacy EAI schema class
// (Customs.Inf.CertificateOfOrigins.EAISchema PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback): element names, their
// namespace, and the IssueDate elements from CertificateOfOriginsUtil.CreateCertificateOfOriginRequestFeedback (GN-01, GN-02,
// GN-04). The expected values are the legacy schema's, not what the worker happens to produce.
[TestFixture]
public class WorkerRequestFeedbackXmlTests
{
    private const string MessageNamespace = "http://malam.com/customs/CertificateOfOrigins/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback";
    private const string QueryUrlTemplate = "https://query.example/CertificateOfOrigin?guid={0}";

    [Test]
    public async Task FeedbackElementsCarryTheLegacySchemaNamesAndNamespace()
    {
        var guid = Guid.NewGuid();
        var xml = await SendFeedback(Dto(guid, isDeclarationReleased: true));

        var feedback = xml.Root!.Element(XName.Get("CertificateOfOriginRequestFeedback", MessageNamespace));
        Assert.That(feedback, Is.Not.Null, "the feedback element sits in the message namespace");
        var names = feedback!.Elements().Select(element => element.Name).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(names.Select(name => name.NamespaceName), Is.All.EqualTo(MessageNamespace),
                "every child element is in the message namespace (legacy: AnonymousType of PC_NG_2281_MSG02)");
            Assert.That(names.Select(name => name.LocalName), Is.EqualTo(new[]
            {
                "internalApplication", "certificateID", "certificateOfOriginTypeCode", "certificateOfOriginStatusCode",
                "FeedbackRemark", "rejectCancelReason", "QueryURL", "requestReasonCode", "IssueDateIfReleased",
            }), "the legacy schema's element names and order");
            Assert.That(feedback.Element(XName.Get("QueryURL", MessageNamespace))!.Value,
                Is.EqualTo(string.Format(QueryUrlTemplate, guid)), "the query URL comes from the CertificateOfOriginQueryURL parameter");
            Assert.That(feedback.Element(XName.Get("IssueDateIfReleased", MessageNamespace))!.Value, Is.EqualTo("2026-09-30"));
            Assert.That(xml.Root.Elements().Select(element => element.Name.LocalName), Does.Not.Contain("Attachment"),
                "no attachment → no element (legacy Attachment[] was not nillable)");
        });
    }

    [Test]
    public async Task NotReleasedDeclarationPutsTheIssuingDateInIssueDateIfNotReleased()
    {
        var xml = await SendFeedback(Dto(Guid.NewGuid(), isDeclarationReleased: false));
        var feedback = xml.Root!.Element(XName.Get("CertificateOfOriginRequestFeedback", MessageNamespace))!;

        Assert.Multiple(() =>
        {
            Assert.That(feedback.Element(XName.Get("IssueDateIfNotReleased", MessageNamespace))?.Value, Is.EqualTo("2026-09-30"));
            Assert.That(feedback.Element(XName.Get("IssueDateIfReleased", MessageNamespace)), Is.Null);
        });
    }

    private static IssueCertificateDto Dto(Guid guid, bool isDeclarationReleased)
    {
        return new IssueCertificateDto
        {
            CertificateOfOriginId = 10,
            CertificateNumber = "COO-10",
            CertificateTypeID = 1,
            CertificateOfOriginStatusId = 4,
            RequestReasonCode = 1,
            CreateCustomerID = 77,
            InternalApplication = "APP-1",
            GUID = guid,
            IssuingDate = new DateTime(2026, 9, 30),
            IsDeclarationReleased = isDeclarationReleased,
        };
    }

    private static async Task<XDocument> SendFeedback(IssueCertificateDto dto)
    {
        OutgoingMessageRequest? sent = null;

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<IssueCertificateOfOriginBL>>(NullLogger<IssueCertificateOfOriginBL>.Instance);
        services.AddSingleton(Fake<ICommonExternalProxy>());
        services.AddSingleton(Fake<IDocumentsExternalProxy>());
        services.AddSingleton<IIssueCertificateDal>(new NoOpDal());
        services.AddSingleton(Fake<ICommonCloudProxy>((method, args) =>
        {
            sent = (OutgoingMessageRequest)args![0]!;
            return Task.CompletedTask;
        }));
        services.AddSingleton(Fake<IParametersUtil>((method, _) =>
            method.Name == nameof(IParametersUtil.Get) ? TaskFromResult(method.ReturnType.GetGenericArguments()[0], QueryUrlTemplate) : null));
        services.AddSingleton(Fake<ICustomerUtil>((method, _) =>
        {
            // GetCustomerById → a customer with a Title (the destination external id).
            var customerType = method.ReturnType.GetGenericArguments()[0];
            var create = typeof(DispatchProxy).GetMethods()
                .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
                .MakeGenericMethod(customerType, typeof(InterfaceFake));
            var customer = create.Invoke(null, null)!;
            ((InterfaceFake)customer).Handler = (customerMethod, _) => customerMethod.Name == "get_Title" ? "CUST-77" : DefaultReturn(customerMethod);
            return TaskFromResult(customerType, customer);
        }));
        services.AddScoped<IssueCertificateOfOriginBL>();
        services.AddScoped<IssueCertificateOfOriginHandler>();

        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IssueCertificateOfOriginHandler>();
        await handler.SendRequestFeedbackAsync(dto, null);

        Assert.That(sent, Is.Not.Null, "the feedback is sent");
        return XDocument.Parse(sent!.MessageXml!);
    }

    private sealed class NoOpDal : IIssueCertificateDal
    {
        public Task UpdateCertificateOfOriginStatus(int certificateOfOriginId, int status) => Task.CompletedTask;

        public Task UpdateIsInPublishingProcess(int certificateOfOriginId, bool value) => Task.CompletedTask;

        public Task<CertificateOfOriginDetailsDto?> GetCertificateOfOriginById(int certificateOfOriginId) => Task.FromResult<CertificateOfOriginDetailsDto?>(null);
    }

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    private static object? DefaultReturn(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            return TaskFromResult(inner, inner.IsValueType ? Activator.CreateInstance(inner) : null);
        }

        return returnType.IsValueType && returnType != typeof(void) ? Activator.CreateInstance(returnType) : null;
    }

    private static object TaskFromResult(Type resultType, object? value)
    {
        return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [value])!;
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
