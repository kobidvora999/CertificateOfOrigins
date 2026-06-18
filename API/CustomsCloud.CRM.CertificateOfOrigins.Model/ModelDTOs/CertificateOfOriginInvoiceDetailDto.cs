namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginInvoiceDetailDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public int? CurrencyTypeId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public DateTimeOffset InvoiceDate { get; set; }
    public string? InvoiceGoodsDescription { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool IsToPrint { get; set; }
    public List<CertificateOfOriginItemDetailDto> CertificateOfOriginItemDetail { get; set; } = new();
}
