using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DocumentsProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Documents), IDocumentsProxy
{
    public async Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("External/GetDocumentsByEntity")
            .AddQueryStringParameter("entityId", entityId)
            .AddQueryStringParameter("entityTypeId", entityTypeId);
        return (await ExecuteAsync<List<DocumentDto>>(req)).Data;
    }
}
