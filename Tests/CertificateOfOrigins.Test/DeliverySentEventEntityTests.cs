using System.Reflection;
using System.Text.Json;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// BN-02: the delivery-sent callback body is the platform EventResponseArgs, bound from JSON as ASP.NET binds it
// (JsonSerializerDefaults.Web). The file is the related entity whose entityType is AuthenticationRequestFile (12385),
// loaded by entityId. The test goes through JSON on purpose: a test that builds the DTO in C# cannot see a wire-name
// mismatch (the old custom DTO read "id", which the platform never sends).
[TestFixture]
public class DeliverySentEventEntityTests
{
    private const int FileId = 5;

    [TestCase("""{ "relatedEntities": [ { "entityId": 5, "entityType": 12385 } ] }""", true)]
    [TestCase("""{ "relatedEntities": [ { "entityId": 9, "entityType": 12386 }, { "entityId": 5, "entityType": 12385 } ] }""", true)]
    [TestCase("""{ "relatedEntities": [ { "entityId": 5, "entityType": 12386 } ] }""", false)]
    [TestCase("""{ "relatedEntities": [ { "id": 5, "entityType": 12385 } ] }""", false)]
    [TestCase("""{ "relatedEntities": [] }""", false)]
    [TestCase("""{ }""", false)]
    public async Task TheFileIsTheRelatedEntityOfTypeAuthenticationRequestFile(string body, bool expected)
    {
        var args = JsonSerializer.Deserialize<EventResponseArgs>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var dataLayer = Fake<ICertificateOfOriginsDal>((method, callArgs) =>
            method.Name == "GetAuthenticationFileById"
                ? Task.FromResult<CertificateOfOriginsImportAuthenticationFileDetails?>((int)callArgs![0]! == FileId ? new CertificateOfOriginsImportAuthenticationFileDetails { Id = FileId } : null)
                : null);
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<AuthenticationRequestBl>>(NullLogger<AuthenticationRequestBl>.Instance);
        services.AddSingleton(dataLayer);
        var bl = new AuthenticationRequestBl(services.BuildServiceProvider(), Fake<IParametersUtil>(), Fake<ILookupUtil>());

        var result = await bl.HandleAuthenticationRequestDeliverySent(args);

        Assert.That(result, Is.EqualTo(expected));
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
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, [inner.IsValueType ? Activator.CreateInstance(inner) : null])!;
        }

        return returnType.IsValueType && returnType != typeof(void) ? Activator.CreateInstance(returnType) : null;
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
