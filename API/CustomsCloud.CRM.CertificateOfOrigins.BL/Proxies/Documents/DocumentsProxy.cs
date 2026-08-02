using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DocumentsProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Documents), IDocumentsProxy
{
    // Legacy: Container.Resolve<IDocumentsExternalProxy>().GetDocumentsByEntitySync(entityId, EEntityType) —
    // the documents attached to an entity live in the Documents microservice.
    public async Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"Document/DocumentsByEntity/{entityId}/{entityTypeId}"); // TODO(blocking): confirm endpoint name/route with the Documents microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<DocumentDto>>();
    }

    // Legacy: Container.Resolve<IDocumentsExternalProxy>().DeleteDocument(docIds, VirtualEntity) — removes the given
    // documents from the entity in the Documents microservice.
    public async Task DeleteDocuments(List<int> documentIds, VirtualEntityDto entity)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Document/DeleteDocuments") // TODO(blocking): confirm endpoint name/route with the Documents microservice
            .AddBody(new { DocumentIds = documentIds, Entity = entity });
        await ExecuteAsync(req);
    }
}
