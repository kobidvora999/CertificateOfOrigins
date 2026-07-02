namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportAuthenticationRequestManufacturingAreaDto
{
    public int Id { get; set; }
    public int ExportAuthenticationRequestId { get; set; }
    public string? ManufacturingArea { get; set; }
    public string? ManufacturingZipcode { get; set; }
}
