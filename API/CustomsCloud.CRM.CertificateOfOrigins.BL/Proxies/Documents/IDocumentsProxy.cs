using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IDocumentsProxy
{
    // Legacy: IDocumentsExternalProxy.GetDocumentsByEntitySync(entityId, eEntityType). Returns the documents
    // attached to an entity from the Documents microservice.
    Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId);
}
