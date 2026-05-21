namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginDetailsDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public int CertificateDetailsTypeCodeId { get; set; }
    public string? Value { get; set; }
    public string? DisplayedValue { get; set; }

    public CertificateDetailsTypeCodeEnumDto? CertificateDetailsTypeCode { get; set; }
}
