namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// An invoice item line of the web-query response (legacy CertificateOfOriginItemDetailDTO — the web contract).
// The legacy always initialized this list and never populated it, so in practice it is always empty
// (developer-confirmed 2026-07-28). The type is kept to preserve the response shape.
public class CertificateOfOriginWebItemDetailDto
{
    public string? ItemNumber { get; set; }

    public string? OriginCriterion { get; set; }

    public string? GoodsDescription { get; set; }

    public decimal GrossWeight { get; set; }

    public string? MeasurementUnitType { get; set; }
}
