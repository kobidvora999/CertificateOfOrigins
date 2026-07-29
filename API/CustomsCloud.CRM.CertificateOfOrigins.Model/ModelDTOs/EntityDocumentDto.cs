namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A document-to-entity link (legacy EntityDocumentDto) — the other entities a document is attached to.
// Populated from each document's EntityDocument collection returned by the Documents service.
public class EntityDocumentDto
{
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }
}
