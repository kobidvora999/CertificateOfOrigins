namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A document returned by GetEntityDocuments (legacy DocumentDTO). The raw fields (Id, TypeId, IsIncoming,
// CreateDate, Title, IsAccepted, IsRequired, ExternalId, StringDynamicParams=raw notes, OtherRelatedEntities)
// come from the Documents microservice via IDocumentsProxy; the BL enriches TypeName (DocumentType lookup) and
// the composed Notes.
public class DocumentDto
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    // BL-enriched from the DocumentType lookup (ILookupUtil). Legacy: SystemTablesUtil.GetCodeById<DocumentType>.Name.
    public string? TypeName { get; set; }

    public bool? IsIncoming { get; set; }

    public DateTime CreateDate { get; set; }

    public string? Title { get; set; }

    public bool IsAccepted { get; set; }

    public bool IsRequired { get; set; }

    // BL-composed: "{Id} {Title} {TypeName}" (legacy parity).
    public string? Notes { get; set; }

    public string? ExternalId { get; set; }

    // The document's raw Notes field (legacy mapped Document.Notes -> DocumentDTO.StringDynamicParams).
    public string? StringDynamicParams { get; set; }

    public List<EntityDocumentDto> OtherRelatedEntities { get; set; } = [];
}
