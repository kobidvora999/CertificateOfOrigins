namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Web-query response for GetCertificateRequestByGuid (Incoming/portal contract). Mirrors the legacy
// CertificateOfOriginsResponse. ExceptionDescription preserves the legacy in-band error contract:
// invalid guid / no matching certificate are returned as an HTTP 200 with this field set (NOT thrown as a
// 404), so the external portal that consumes GetPC_Web_9096_CertificateRequest is unaffected.
public class CertificateOfOriginsResponseDto
{
    public string? CertificateNumber { get; set; }

    public string? QueryUrl { get; set; }

    public int DocumentId { get; set; }

    public List<FieldDataDto> CertificateOfOriginDetails { get; set; } = [];

    public List<CertificateOfOriginWebInvoiceDetailDto> CertificateOfOriginInvoiceDetails { get; set; } = [];

    public string? ExceptionDescription { get; set; }
}
