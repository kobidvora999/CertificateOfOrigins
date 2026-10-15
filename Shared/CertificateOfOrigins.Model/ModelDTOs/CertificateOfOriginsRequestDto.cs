using System.ComponentModel.DataAnnotations;

namespace CertificateOfOrigins.Model.ModelDTOs;

// Web-query request for GetCertificateRequestByGuid (Incoming/portal contract GetPC_Web_9096_CertificateRequest).
// The certificate is located either by its guid, or by CertificateOfOriginNumber + IssuingDate.
//
// The two text fields keep an empty value as an empty string: MVC binding turns "" into null by default, but the legacy
// serializer kept "" as a non-null string, so an empty guid failed the guid parse and answered "Invalid Guid" instead
// of falling through to the number + date lookup.
public class CertificateOfOriginsRequestDto
{
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? CertificateOfOriginGuid { get; set; }

    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? CertificateOfOriginNumber { get; set; }

    public DateTime? IssuingDate { get; set; }
}
