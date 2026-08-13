namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The certificate feedback body (legacy PC_NG_2281_MSG02 CertificateOfOriginRequestFeedback) — echoes the certificate's
// identity/status back to the agent, the public query URL, and the issuing dates (split by whether the export
// declaration was released).
public class CertificateOfOriginRequestFeedbackDto
{
    public string? InternalApplication { get; set; }

    public string? CertificateId { get; set; }

    public int CertificateOfOriginTypeCode { get; set; }

    public int CertificateOfOriginStatusCode { get; set; }

    public string? FeedbackRemark { get; set; }

    public string? RejectCancelReason { get; set; }

    public string? QueryUrl { get; set; }

    public int RequestReasonCode { get; set; }

    // Set only when the export declaration is released; otherwise IssueDateIfNotReleased carries the date.
    public DateTime? IssueDateIfReleased { get; set; }

    public DateTime? IssueDateIfNotReleased { get; set; }
}
