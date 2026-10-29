using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// Web-query response for GetCertificateRequestByGuid (Incoming/portal contract). Mirrors the legacy
// CertificateOfOriginsResponse. ExceptionDescription preserves the legacy in-band error contract:
// invalid guid / no matching certificate are returned as an HTTP 200 with this field set (NOT thrown as a
// 404), so the external portal that consumes GetPC_Web_9096_CertificateRequest is unaffected.
//
// The wire names are the legacy ones, PascalCase and with the legacy spelling "ExeptionDescription" (parity D-F1,
// decided 2026-10-11): a portal adapter that maps fields by name keeps finding the error text. The legacy
// BaseWebResponse envelope (Status, TechnicalInformation, SevriceInstanceID, CallID) is not reproduced.
public class CertificateOfOriginsResponseDto
{
    [JsonPropertyName("CertificateNumber")]
    public string? CertificateNumber { get; set; }

    [JsonPropertyName("QueryURL")]
    public string? QueryUrl { get; set; }

    [JsonPropertyName("DocumentID")]
    public int DocumentId { get; set; }

    // Null on the "Invalid Guid" / "No Matching Certificate" answers, as legacy (the lists were never set there), so a portal
    // that tells an error by a null list does not read the answer as an empty success (parity finding DN-01).
    [JsonPropertyName("CertificateOfOriginDetails")]
    public List<FieldDataDto>? CertificateOfOriginDetails { get; set; }

    [JsonPropertyName("CertificateOfOriginInvoiceDetails")]
    public List<CertificateOfOriginWebInvoiceDetailDto>? CertificateOfOriginInvoiceDetails { get; set; }

    [JsonPropertyName("ExeptionDescription")]
    public string? ExceptionDescription { get; set; }
}
