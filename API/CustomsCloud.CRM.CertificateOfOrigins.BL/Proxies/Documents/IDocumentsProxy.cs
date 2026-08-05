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

    // Legacy: SELECT ... FROM Infrastructure.Docs_Document WHERE ID = @DocumentID (GetAuthenticationRequestByID SP
    // result-set #3) — a single document by its id from the Documents microservice.
    Task<DocumentDto?> GetDocumentById(int documentId);

    // Legacy: IDocumentServiceAdapter.AttachDocumentsToEntity([DocumentsToEntityDTO]). Attaches the given documents
    // to an entity in the Documents microservice.
    Task AttachDocumentsToEntity(DocumentsToEntityDto documents);
}
