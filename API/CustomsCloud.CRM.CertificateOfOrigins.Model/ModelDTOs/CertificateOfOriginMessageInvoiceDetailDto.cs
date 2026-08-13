namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// One invoice line of the incoming certificate message (PC_NG_2280_MSG01 CertificateOfOriginRequestInvoiceDetail),
// with its item detail lines.
public class CertificateOfOriginMessageInvoiceDetailDto
{
    public List<CertificateOfOriginMessageItemDetailDto> ItemDetails { get; set; } = [];

    public string? InvoiceNum { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal? InvoiceSum { get; set; }

    public string? CurrencyType { get; set; }

    public string? DescriptionOfInvoice { get; set; }

    public bool IsInvoicesForPrint { get; set; }
}
