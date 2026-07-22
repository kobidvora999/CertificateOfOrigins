namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors the legacy WCF GoodsItemCerificateDTO (typo in "Cerificate" preserved for contract parity).
public class GoodsItemCerificateDto
{
    public int GoodsItemId { get; set; }
    public string? CertificateNumber { get; set; }
    public int? CertificateOfOriginId { get; set; }
}
