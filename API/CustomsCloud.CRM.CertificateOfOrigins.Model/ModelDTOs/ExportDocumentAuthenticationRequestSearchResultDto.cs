namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportDocumentAuthenticationRequestSearchResultDto
{
    public int RequestId { get; set; }
    public string? CountryName { get; set; }
    public string? ForeignCustomsHouseName { get; set; }
    public string? DocumentTypeName { get; set; }
    public string? ExportDeclarationTitle { get; set; }
    public string? RequestIssuerName { get; set; }
    public string? RequestStatusName { get; set; }
    public int CustomerId { get; set; }
    public int? ExportLeadDocumentId { get; set; }

    // Enrichment carriers (not part of the legacy WCF result) — used by the BL to fill the display names
    public int? CountryId { get; set; }
    public int? ExporterCustomerId { get; set; }
}
