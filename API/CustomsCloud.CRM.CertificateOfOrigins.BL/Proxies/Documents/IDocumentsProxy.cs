using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IDocumentsProxy
{
    // Legacy: IDocumentsExternalProxy.GetDocumentsByEntitySync(entityId, eEntityType). Returns the documents
    // attached to an entity from the Documents microservice.
    Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId);

    // Legacy: IDocumentsExternalProxy.DeleteDocument(docIds, VirtualEntity). Removes the given documents from the
    // entity in the Documents microservice.
    Task DeleteDocuments(List<int> documentIds, VirtualEntityDto entity);
}
