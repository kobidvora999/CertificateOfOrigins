namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Web-query request for GetCertificateRequestByGuid (Incoming/portal contract GetPC_Web_9096_CertificateRequest).
// The certificate is located either by its guid, or by CertificateOfOriginNumber + IssuingDate.
public class CertificateOfOriginsRequestDto
{
    public string? CertificateOfOriginGuid { get; set; }

    public string? CertificateOfOriginNumber { get; set; }

    public DateTime? IssuingDate { get; set; }
}
