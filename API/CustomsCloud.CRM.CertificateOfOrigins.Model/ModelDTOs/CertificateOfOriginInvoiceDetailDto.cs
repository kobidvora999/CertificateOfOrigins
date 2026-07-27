namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 5 of dbo.GetCertificateOfOriginByID — the certificate's invoices. Each invoice's item lines
// (result set 6) are nested in the materializer by CertificateOfOriginInvoiceDetailId.
public class CertificateOfOriginInvoiceDetailDto
{
    public int Id { get; set; }
    public int CertificateOfOriginId { get; set; }
    public int? CurrencyTypeId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string? InvoiceGoodsDescription { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool IsToPrint { get; set; }
    public List<CertificateOfOriginItemDetailDto> CertificateOfOriginItemDetail { get; set; } = [];
}
