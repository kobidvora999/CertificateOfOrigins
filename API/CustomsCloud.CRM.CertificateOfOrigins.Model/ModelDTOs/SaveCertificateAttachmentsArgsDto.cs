namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy SaveCertificateAttachmentsArgsDTO — the payload for SaveCertificateOfOriginAttachments. Carries the
// generated certificate template(s) plus the metadata used to build each attachment's title/filename and to decide
// the draft/final label.
public class SaveCertificateAttachmentsArgsDto
{
    // The generated template document(s) to save as attachments on the certificate.
    public List<TemplateResultDto> CertificatesTemplates { get; set; } = [];

    public string? CertificateNumber { get; set; }

    // The certificate entity id the attachments are linked to (the document EntityId).
    public int CertificateId { get; set; }

    // Used only to pick the Draft/Final title label (== ERequestReason.Draft).
    public int CertificateRequestReasonCode { get; set; }

    // Resolves the certificate-type display name (via ECertificateOfOriginType) for the title/filename.
    public int CertificateTypeId { get; set; }

    // Alternate draft sentinel ("isDraft") also checked for the Draft/Final label.
    public string? AdditionalInfo { get; set; }
}
