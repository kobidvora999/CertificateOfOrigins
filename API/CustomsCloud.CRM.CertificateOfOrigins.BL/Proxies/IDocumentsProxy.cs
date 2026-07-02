using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IDocumentsProxy
{
    // Returns the documents attached to a given entity (Documents service: External/GetDocumentsByEntity).
    Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId);
}
