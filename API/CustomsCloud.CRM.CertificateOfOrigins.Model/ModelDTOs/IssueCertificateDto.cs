namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Queue message payload for the "IssueCertificateOfOrigin" exchange (issue-by-worker flow).
// Field set preserved 1:1 from the legacy IssueCertificateDto — the consuming worker relies on it.
public class IssueCertificateDto
{
    public int ReportId { get; set; }

    public int CertificateOfOriginId { get; set; }

    public string? CertificateNumber { get; set; }

    public int CertificateOfOriginStatusId { get; set; }

    public int CertificateTypeId { get; set; }

    public string? CertificateTypeName { get; set; }

    public int RequestReasonCode { get; set; }

    public bool IsInPublishingProcess { get; set; }

    public int CreateCustomerId { get; set; }

    public string? RejectCancelReason { get; set; }

    public string? InternalApplication { get; set; }

    public string? FeedbackRemark { get; set; }

    public DateTime? IssuingDate { get; set; }

    public bool? IsDeclarationReleased { get; set; }

    public Guid? Guid { get; set; }

    public int OrganizationUnitId { get; set; }
}
