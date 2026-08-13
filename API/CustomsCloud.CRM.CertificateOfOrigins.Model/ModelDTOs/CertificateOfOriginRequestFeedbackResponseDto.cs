namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The synchronous feedback returned for an incoming certificate-of-origin request (legacy PC_NG_2281_MSG02). Carries the
// certificate feedback, the resolved application (certificate) id, any declaration-reconciliation exceptions, and the
// rendered attachment(s) when the certificate was published.
public class CertificateOfOriginRequestFeedbackResponseDto
{
    // The saved certificate id (legacy ResponseContentHeader.ApplicationID).
    public int ApplicationId { get; set; }

    public CertificateOfOriginRequestFeedbackDto Feedback { get; set; } = new();

    // Declaration-reconciliation exceptions (from the post-save declaration-submitted check); null/empty when all matched.
    public List<CertificateOfOriginExceptionDto>? Exceptions { get; set; }

    // Rendered certificate document(s); null unless the certificate was published and the reason code carries attachments.
    public List<CertificateOfOriginMessageAttachmentDto>? Attachments { get; set; }
}
