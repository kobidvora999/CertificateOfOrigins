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

    public async Task<List<DocumentDto>?> GetDocumentsByIds(List<int> documentIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Internal/GetDocumentsByIds") // TODO: confirm endpoint name with the Documents microservice
            .AddBody(documentIds);
        return (await ExecuteAsync<List<DocumentDto>>(req)).Data;
    }

    public async Task<bool> DeleteDocuments(List<int> documentIds, int entityId, int entityTypeId)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("External/DeleteDocuments") // TODO: confirm endpoint name with the Documents microservice
            .AddQueryStringParameter("entityId", entityId)
            .AddQueryStringParameter("entityTypeId", entityTypeId)
            .AddBody(documentIds);
        return (await ExecuteAsync<bool>(req)).Data;
    }

    public async Task<DocumentDto?> UploadDocumentAndSave(DocumentDto document, byte[] content)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("External/UploadDocumentAndSave") // TODO: confirm endpoint name + payload shape with the Documents microservice
            .AddBody(new { Document = document, Content = content });
        return (await ExecuteAsync<DocumentDto>(req)).Data;
    }
}
