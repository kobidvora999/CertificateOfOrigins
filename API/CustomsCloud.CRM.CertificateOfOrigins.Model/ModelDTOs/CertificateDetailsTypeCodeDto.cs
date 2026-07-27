namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 3 of dbo.GetCertificateOfOriginByID — the certificate-details type-code lookup rows
// (CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode). Not returned as its own collection; each row is
// attached to the matching detail (result set 4) via CertificateDetailsTypeCodeId.
public class CertificateDetailsTypeCodeDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int State { get; set; }
    public string? Description { get; set; }
    public string? EnglishName { get; set; }
    public string? Enumeration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
    public string? DetailTypeFormat { get; set; }
    public int DataTypeId { get; set; }
}
