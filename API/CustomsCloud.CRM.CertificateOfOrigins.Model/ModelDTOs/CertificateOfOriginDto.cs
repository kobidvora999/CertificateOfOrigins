namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Full certificate-of-origin graph returned by GetCertificateOfOriginById (dbo.GetCertificateOfOriginByID —
// a 7-result-set SP). Header columns (result set 1) + computed StakeholdersIds (exporter CustomerId +
// customs-agent CreateCustomerId) + the child collections composed from the remaining result sets.
public class CertificateOfOriginDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string? Title { get; set; }
    public int State { get; set; }
    public byte[]? TimeStamp { get; set; }
    public DateTime CreateDate { get; set; }
    public int CreateUserId { get; set; }
    public DateTime UpdateDate { get; set; }
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
    public DateTime? IssuingDate { get; set; }
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
    public int VersionNumber { get; set; }
    public bool IsLastVersion { get; set; }
    public int? ApproveUserId { get; set; }
    public bool IsInPublishingProcess { get; set; }

    public List<int> StakeholdersIds { get; set; } = [];
    public List<CertificateMilestoneDto> Milestones { get; set; } = [];
    public List<CertificateOfOriginDetailDto> CertificateOfOriginDetails { get; set; } = [];
    public List<CertificateOfOriginVsDeclarationErrorDto> CertificateOfOriginVsDeclarationError { get; set; } = [];
    public List<CertificateOfOriginInvoiceDetailDto> CertificateOfOriginInvoiceDetail { get; set; } = [];
}
