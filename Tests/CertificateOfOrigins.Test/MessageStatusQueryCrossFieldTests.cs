using System.Collections;
using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// CN-01: legacy message 2280 ran the cross-field pass (CheckFields) only when the reason is neither GetRequestStatus nor
// CertificateCancellation (IncomingMessageServicePartial.cs:95-98). A status query whose body breaks a cross-field rule
// got its status back in legacy; it must not be answered with that rule's error.
[TestFixture]
public class MessageStatusQueryCrossFieldTests
{
    private const int FieldMandatoryWhenTheAnotherField = 4832;

    [Test]
    public async Task StatusQueryDoesNotRunTheCrossFieldRules()
    {
        var codes = await ValidateBody((int)ERequestReason.GetRequestStatus);

        Assert.That(codes, Does.Not.Contain(FieldMandatoryWhenTheAnotherField));
    }

    // The control: the same body on a create reason does break the rule, so the test above proves the gate, not a body that
    // never triggers it.
    [Test]
    public async Task CreateReasonRunsTheCrossFieldRules()
    {
        var codes = await ValidateBody((int)ERequestReason.NewCertificate);

        Assert.That(codes, Does.Contain(FieldMandatoryWhenTheAnotherField));
    }

    private static async Task<List<int>> ValidateBody(int reason)
    {
        var request = new CertificateOfOriginRequestMessageDto
        {
            AgentRequest = new CertificateOfOriginAgentRequestDto
            {
                RequestReasonCode = reason,
                CertificateOfOriginTypeCode = 1,
                CertificateId = "COO-1",
            },
            // InSufficentworkingInd = true without InsufficentWorkingText breaks a cross-field rule (legacy CheckConditionalFields).
            CertificateOfOrigin = new CertificateOfOriginMessageDto
            {
                ExporterName = "Exporter",
                InSufficentworkingInd = true,
                InsufficentWorkingText = null,
            },
        };

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, _) => method.Name switch
        {
            "GetCertificateTypeCode" => Task.FromResult<CertificateOfOriginTypeCode?>(new CertificateOfOriginTypeCode()),
            // One optional, free-text field, so the message produces a detail row and the cross-field pass can run.
            "GetDetailsPerCertificate" => Task.FromResult(new List<DetailsPerCertificate>
            {
                new() { CertificateDetailsTypeCodeId = (int)ECertificateDetailsType.ExporterName, ConstraintTypeEnumId = (int)EConstraintType.Optional },
            }),
            _ => null,
        });

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<CertificateOfOriginsBl>>(NullLogger<CertificateOfOriginsBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>());
        var serviceProvider = services.BuildServiceProvider();
        var bl = new CertificateOfOriginsBl(serviceProvider, Fake<ILookupUtil>(), Fake<IParametersUtil>());

        var contextType = typeof(CertificateOfOriginsBl).GetNestedType("MessageValidationContext", BindingFlags.NonPublic)!;
        var context = Activator.CreateInstance(contextType, nonPublic: true)!;
        var requestExceptions = new List<CertificateOfOriginExceptionDto>();
        var validate = typeof(CertificateOfOriginsBl).GetMethod("ValidateMessageBody", BindingFlags.NonPublic | BindingFlags.Instance)!;
        await (Task)validate.Invoke(bl, [request, context, requestExceptions])!;

        var contextExceptions = ((IEnumerable)contextType.GetProperty("Exceptions")!.GetValue(context)!).Cast<CertificateOfOriginExceptionDto>();
        var result = contextExceptions.Concat(requestExceptions).Select(exception => exception.ExceptionType).ToList();
        return result;
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
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            var value = inner.IsValueType ? Activator.CreateInstance(inner) : (inner.IsGenericType && inner.GetGenericTypeDefinition() == typeof(List<>) ? Activator.CreateInstance(inner) : null);
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, [value])!;
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
