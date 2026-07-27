namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 2 of dbo.GetCertificateOfOriginByID — validation errors between the certificate and its export
// declaration.
public class CertificateOfOriginVsDeclarationErrorDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public string? ErrorText { get; set; }
    public int State { get; set; }
}
