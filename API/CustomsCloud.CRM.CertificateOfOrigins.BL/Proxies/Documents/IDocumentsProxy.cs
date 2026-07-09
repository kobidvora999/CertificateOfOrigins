using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IDocumentsProxy
{
    // Returns the documents attached to a given entity (Documents service: Documents/DocumentsByEntity).
    Task<List<DocumentDto>?> GetDocumentsByEntity(int entityId, int entityTypeId);

    Task<List<DocumentDto>?> GetDocumentsByIds(List<int> documentIds);

    Task<bool> DeleteDocuments(List<int> documentIds, int entityId, int entityTypeId);

    Task<DocumentDto?> UploadDocumentAndSave(DocumentDto document, byte[] content);
}
