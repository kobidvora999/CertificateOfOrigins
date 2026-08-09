namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The payload published to the "IssueCertificateOfOrigin" RabbitMQ exchange when a certificate is issued by a worker
// (SaveCertificateOfOrigin, on publish, when IssueCertificateOfOriginByWorker is on). Mirrors the legacy IssueCertificateDto.
public class IssueCertificateDto
{
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

    public Guid? Guid { get; set; }

    public int OrganizationUnitId { get; set; }
}
