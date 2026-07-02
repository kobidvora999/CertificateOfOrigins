namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ImportAuthenticationRequestFilterDto
{
    public int? PrefernceDocumentType { get; set; }
    public int? GoodsOrigionCountry { get; set; }
    public int? IssuingCountry { get; set; }
    public int? ImportCountry { get; set; }
    public DateTimeOffset? FromRequestDate { get; set; }
    public DateTimeOffset? ToRequestDate { get; set; }
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
