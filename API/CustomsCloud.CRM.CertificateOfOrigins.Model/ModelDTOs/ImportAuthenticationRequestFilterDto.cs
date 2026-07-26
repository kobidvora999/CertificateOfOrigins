namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Filter for dbo.GetImportAuthenticationRequestByFilter. All predicates are optional except the request-date
// range (FromRequestDate/ToRequestDate) which the SP always applies (BETWEEN) — callers should supply both.
// Legacy typos in the SP @param names (@PrefernceDocumentType, @GoodsOrigionCountry) are corrected here; the
// BL maps each field to the SP parameter explicitly, so the DTO stays clean.
public class ImportAuthenticationRequestFilterDto
{
    public int? PreferenceDocumentType { get; set; }
    public int? GoodsOriginCountry { get; set; }
    public int? IssuingCountry { get; set; }
    public int? ImportCountry { get; set; }
    public DateTime? FromRequestDate { get; set; }
    public DateTime? ToRequestDate { get; set; }
    public int? CustomsHouseId { get; set; }
    public int? RequestReason { get; set; }
    public int? LeadDocumentId { get; set; }
    public int? ImporterId { get; set; }
    public int? VendorId { get; set; }
    public int? DecisionId { get; set; }
    public int? CustomerId { get; set; }
    public int? DocumentId { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? DocumentNumber { get; set; }
    public int? AuthenticationFileId { get; set; }
    public int? CreateUserId { get; set; }
}
