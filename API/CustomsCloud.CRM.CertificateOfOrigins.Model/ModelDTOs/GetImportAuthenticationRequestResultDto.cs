namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class GetImportAuthenticationRequestResultDto
{
    public int? DocumentId { get; set; }
    public string? IssuingCountryId { get; set; }
    public string? OrganizationUnitId { get; set; }
    public string? PreferenceDocumentTypeId { get; set; }
    public int? AuthenticationFileId { get; set; }
    public string? LeadDocumentTitle { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string? VendorName { get; set; }
    public int? IssuingCountryIdNum { get; set; }
    public int? OrganizationUnitIdNum { get; set; }
    public string? ResponseNameEmail { get; set; }
    public int? LeadDocumentId { get; set; }
    public int? CustomerId { get; set; }
    public int? VendorId { get; set; }
    public int? DecisionId { get; set; }
    public string? ImporterName { get; set; }
    public int? AuthenticationFileStatusId { get; set; }
}
