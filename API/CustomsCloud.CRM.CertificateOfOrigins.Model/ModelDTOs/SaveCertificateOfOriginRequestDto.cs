using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for SaveCertificateOfOrigin — the certificate as edited by the SPA (the legacy passed the full
// CertificateOfOrigin entity). Carries the editable scalar fields, its detail child collection, and the load-time
// snapshots used for change detection (the .NET 10 stateless replacement for the legacy ChangeTracker.OriginalValues):
// OriginalCertificateOfOriginStatusId + OriginalFeedbackRemark. Id == 0 → new certificate (insert); otherwise update.
public class SaveCertificateOfOriginRequestDto
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public string? Title { get; set; }

    public int State { get; set; }

    public byte[]? TimeStamp { get; set; }

    public int OrganizationUnitId { get; set; }

    public int CustomerId { get; set; }

    public int CreateCustomerId { get; set; }

    public int UpdateCustomerId { get; set; }

    public int? LeadDocumentId { get; set; }

    public int? CertificateIdToCancel { get; set; }

    public string? CertificateNumber { get; set; }

    public int CertificateOfOriginStatusId { get; set; }

    // Load-time snapshot of the status — drives the status-change events (replaces ChangeTracker original value).
    public int OriginalCertificateOfOriginStatusId { get; set; }

    public int? DestinationCountry { get; set; }

    public string? FeedbackRemark { get; set; }

    // Load-time snapshot of the feedback remark — drives the "remarks changed → send feedback" branch.
    public string? OriginalFeedbackRemark { get; set; }

    public string? InternalApplication { get; set; }

    public DateTime? IssuingDate { get; set; }

    public string? RejectCancelReason { get; set; }

    public string? ReplacementReason { get; set; }

    public int RequestReasonCode { get; set; }

    public string? ExportDeclarationNumber { get; set; }

    public string? CertificateToReplaceInImport { get; set; }

    public Guid? Guid { get; set; }

    public string? QrCodePath { get; set; }

    // The generated QR image bytes (populated server-side on publish; round-tripped otherwise).
    public byte[]? QrImage { get; set; }

    public bool IsAttachedList { get; set; }

    public bool? InSufficentworkingInd { get; set; }

    public string? InsufficentWorkingText { get; set; }

    public int VersionNumber { get; set; }

    public bool IsLastVersion { get; set; }

    public int? ApproveUserId { get; set; }

    public bool IsInPublishingProcess { get; set; }

    // The certificate's editable detail rows (country / trade-agreement / date / site fields). Persisted diff-merge by Id.
    public List<CertificateOfOriginDetailDto> CertificateOfOriginDetails { get; set; } = [];

    // The certificate's invoice rows (each with its nested item rows), for the incoming-message create branch. The SPA
    // save path leaves this empty; when populated the DAL diff-merges the invoice + item graph by surrogate id.
    public List<CertificateOfOriginInvoiceDetail> CertificateOfOriginInvoiceDetails { get; set; } = [];
}
