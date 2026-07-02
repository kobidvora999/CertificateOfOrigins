namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors the Documents microservice DocumentDto (CustomsCloud.Infrastructure.Documents.Model.DTO.DocumentDto)
// — the response shape of External/GetDocumentsByEntity. Also reused as the SP projection for the
// authentication-request document result set (extra fields stay null there).
public class DocumentDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string? TypeName { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string? Notes { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalIdNum { get; set; }
    public bool? IsIncoming { get; set; }
    public bool IsRequired { get; set; }
    public bool IsAccepted { get; set; }
    public string? StringDynamicParams { get; set; }
    public List<EntityDocumentDto>? OtherRelatedEntities { get; set; }

    // Not returned by the SP — set in BL from DocumentType.Name (lookup by TypeId)
    public string? FileUrl { get; set; }

    // Upload metadata (used when persisting documents via the Documents service)
    public string? FileName { get; set; }
    public int? OrganizationUnitId { get; set; }
    public int? EntityId { get; set; }
    public int? EntityTypeId { get; set; }
    public List<DocumentAdditionalFieldValueDto>? DocumentAdditionalFieldValues { get; set; }
}
