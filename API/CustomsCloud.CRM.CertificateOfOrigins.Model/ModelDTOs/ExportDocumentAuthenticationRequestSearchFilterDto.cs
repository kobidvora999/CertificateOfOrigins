namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Filter for dbo.ExportDocumentAuthenticationRequestSearch. Only the fields the legacy SP actually consumes are
// included — the WCF filter's UI-only fields (RequestIssuerName-as-filter, DocumentIssueDate*, ExportLeadDocument*,
// EntityTypes) were never sent to the SP and are intentionally omitted.
// ExportDeclarationId is carried for contract fidelity but is a dead parameter in the SP (never referenced).
public class ExportDocumentAuthenticationRequestSearchFilterDto
{
    public int? CountryId { get; set; }
    public int? DocumentTypeId { get; set; }
    public int? RequestId { get; set; }
    public int? ForeignCustomsHouseId { get; set; }
    public int? ExportDeclarationId { get; set; }   // dead in the SP (declared, never used) — kept for fidelity
    public DateTime? RequestOpenDateFrom { get; set; }
    public DateTime? RequestOpenDateTo { get; set; }
    public int? ExportAuthenticationDocumentId { get; set; }
    public string? InvoiceIdNum { get; set; }
    public string? MainDocumentTitle { get; set; }
    public int? ExporterCustomerId { get; set; }
    public int? ExportAuthenticationRequestStatusId { get; set; }
    public int? CreateUserId { get; set; }
}
