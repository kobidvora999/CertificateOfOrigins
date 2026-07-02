namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportDocumentAuthenticationRequestSearchFilterDto
{
    public int? CountryId { get; set; }
    public int? DocumentTypeId { get; set; }
    public int? RequestId { get; set; }
    public int? ForeignCustomsHouseCustomerId { get; set; }

    // legacy: sent to the SP but never used in any filter branch — preserved for contract parity
    public int? ExportDeclarationId { get; set; }
    public DateTime? RequestOpenDateFrom { get; set; }
    public DateTime? RequestOpenDateTo { get; set; }
    public int? ExportAuthenticationDocumentId { get; set; }
    public string? InvoiceIdNum { get; set; }
    public string? MainDocumentTitle { get; set; }
    public int? ExporterId { get; set; }
    public int? ExportAuthenticationRequestStatusId { get; set; }
    public int? CreateUserId { get; set; }
}
