using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// An invoice item line of the web-query response (legacy CertificateOfOriginItemDetailDTO — the web contract).
// The legacy always initialized this list and never populated it, so in practice it is always empty
// (developer-confirmed 2026-07-28). The type is kept to preserve the response shape. Legacy wire names (parity D-F1).
public class CertificateOfOriginWebItemDetailDto
{
    [JsonPropertyName("ItemNumber")]
    public string? ItemNumber { get; set; }

    [JsonPropertyName("OriginCriterion")]
    public string? OriginCriterion { get; set; }

    [JsonPropertyName("GoodsDescription")]
    public string? GoodsDescription { get; set; }

    [JsonPropertyName("GrossWeight")]
    public decimal GrossWeight { get; set; }

    [JsonPropertyName("MeasurementUnitType")]
    public string? MeasurementUnitType { get; set; }
}
