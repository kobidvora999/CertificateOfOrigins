namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginVsDeclarationErrorDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public string? ErrorText { get; set; }
    public int State { get; set; }
}
