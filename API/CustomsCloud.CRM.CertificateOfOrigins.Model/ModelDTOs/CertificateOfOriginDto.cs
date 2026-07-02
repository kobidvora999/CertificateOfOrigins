namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string? Title { get; set; }
    public int State { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
    public int UpdateUserId { get; set; }
    public int OrganizationUnitId { get; set; }
    public int CustomerId { get; set; }
    public int CreateCustomerId { get; set; }
    public int UpdateCustomerId { get; set; }
    public int? LeadDocumentId { get; set; }
    public int? CertificateIdToCancel { get; set; }
    public string? CertificateNumber { get; set; }
    public int CertificateOfOriginStatusId { get; set; }
    public int? DestinationCountry { get; set; }
    public string? FeedbackRemark { get; set; }
    public string? InternalApplication { get; set; }
    public DateTimeOffset? IssuingDate { get; set; }
    public string? RejectCancelReason { get; set; }
    public string? ReplacementReason { get; set; }
    public int RequestReasonCode { get; set; }
    public string? ExportDeclarationNumber { get; set; }
    public string? CertificateToReplaceInImport { get; set; }
    public Guid? Guid { get; set; }
    public string? QrCodePath { get; set; }
    public bool IsAttachedList { get; set; }
    public bool? InSufficentworkingInd { get; set; }
    public string? InsufficentWorkingText { get; set; }
    public byte[]? QrImage { get; set; }
    public int? ApproveUserId { get; set; }
    public int VersionNumber { get; set; }
    public bool IsLastVersion { get; set; }
    public bool IsInPublishingProcess { get; set; }
    public string? CertificateOfOriginTypeCodeName { get; set; }
    public int DocumentId { get; set; }
    public bool? IsDeclarationReleased { get; set; }
    public bool? IsCargoExitedOfCustomsRegulation { get; set; }
    public bool? IsDeclarationReleasedAndNotRetrospectiveCertificate { get; set; }
    public List<int> StakeholdersIds { get; set; } = new();
    public List<CertificateMilestonesDto> Milestones { get; set; } = new();
    public List<CertificateOfOriginVsDeclarationErrorDto> CertificateOfOriginVsDeclarationError { get; set; } = new();
    public List<CertificateOfOriginDetailsDto> CertificateOfOriginDetails { get; set; } = new();
    public List<CertificateOfOriginInvoiceDetailDto> CertificateOfOriginInvoiceDetail { get; set; } = new();
}
