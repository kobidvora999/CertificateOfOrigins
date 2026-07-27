namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 4 of dbo.GetCertificateOfOriginByID — the certificate's detail values. Each detail's type-code
// (result set 3) is attached in the materializer by CertificateDetailsTypeCodeId.
public class CertificateOfOriginDetailDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public int CertificateDetailsTypeCodeId { get; set; }
    public string? Value { get; set; }
    public string? DisplayedValue { get; set; }
    public CertificateDetailsTypeCodeDto? CertificateDetailsTypeCode { get; set; }
}
