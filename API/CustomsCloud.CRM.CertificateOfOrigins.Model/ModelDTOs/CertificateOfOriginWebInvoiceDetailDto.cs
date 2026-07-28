namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// An invoice line of the web-query response (legacy CertificateOfOriginInvoiceDetailDTO — the web contract, not
// the internal one). CurrencyCode was resolved from a currency-type lookup in the legacy; that lookup type is
// not available in the platform, so it is deferred (see the BL — TODO(blocking)).
// CertificateOfOriginItemDetails is always empty (legacy quirk, developer-confirmed 2026-07-28).
public class CertificateOfOriginWebInvoiceDetailDto
{
    public string? InvoiceNumber { get; set; }

    public DateTime InvoiceDate { get; set; }

    public decimal InvoiceAmount { get; set; }

    public string? CurrencyCode { get; set; }

    public string? InvoiceGoodsDescription { get; set; }

    public List<CertificateOfOriginWebItemDetailDto> CertificateOfOriginItemDetails { get; set; } = [];
}
