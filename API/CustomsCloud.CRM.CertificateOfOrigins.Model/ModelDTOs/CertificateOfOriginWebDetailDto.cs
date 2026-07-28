namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 3 of dbo.GetCertificateOfOriginDataForWebQuery — a certificate detail value. In the materializer
// each detail is enriched with its type-code (result set 4, by CertificateDetailsTypeCodeId) and its web
// print-out row (result set 5, by the same id).
public class CertificateOfOriginWebDetailDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public int CertificateDetailsTypeCodeId { get; set; }
    public string? Value { get; set; }
    public string? DisplayedValue { get; set; }

    public CertificateDetailsTypeCodeDto? CertificateDetailsTypeCode { get; set; }
    public CertificateOfOriginWebPrintOutDto? CertificateOfOriginWebPrintOut { get; set; }
}
