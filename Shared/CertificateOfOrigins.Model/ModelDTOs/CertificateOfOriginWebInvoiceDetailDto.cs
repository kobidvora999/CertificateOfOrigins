using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// An invoice line of the web-query response (legacy CertificateOfOriginInvoiceDetailDTO — the web contract, not
// the internal one). CurrencyCode is resolved from the currency-type lookup, as legacy (the local CurrencyType lookup).
// CertificateOfOriginItemDetails is always empty (legacy quirk, developer-confirmed 2026-07-28).
// Legacy wire names (parity D-F1), including the legacy field name CertificateOfOriginItemDetailDTOs.
public class CertificateOfOriginWebInvoiceDetailDto
{
    [JsonPropertyName("InvoiceNumber")]
    public string? InvoiceNumber { get; set; }

    // Date-only on the wire, as the legacy portal contract ("yyyy-MM-dd"; parity finding D F2).
    [JsonPropertyName("InvoiceDate")]
    [JsonConverter(typeof(WebDateJsonConverter))]
    public DateTime InvoiceDate { get; set; }

    [JsonPropertyName("InvoiceAmount")]
    public decimal InvoiceAmount { get; set; }

    [JsonPropertyName("CurrencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("InvoiceGoodsDescription")]
    public string? InvoiceGoodsDescription { get; set; }

    [JsonPropertyName("CertificateOfOriginItemDetailDTOs")]
    public List<CertificateOfOriginWebItemDetailDto> CertificateOfOriginItemDetails { get; set; } = [];
}
