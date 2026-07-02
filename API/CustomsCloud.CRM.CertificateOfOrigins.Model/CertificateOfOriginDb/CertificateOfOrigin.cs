using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_CertificateOfOrigin", Schema = "CRM")]
public class CertificateOfOrigin
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public int State { get; set; }

    [Timestamp]
    public byte[] TimeStamp { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreateDate { get; set; }

    [Column("CreateUserID")]
    public int CreateUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdateDate { get; set; }

    [Column("UpdateUserID")]
    public int UpdateUserId { get; set; }

    [Column("OrganizationUnitID")]
    public int OrganizationUnitId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("CreateCustomerID")]
    public int CreateCustomerId { get; set; }

    [Column("UpdateCustomerID")]
    public int UpdateCustomerId { get; set; }

    [Column("LeadDocumentID")]
    public int? LeadDocumentId { get; set; }

    [Column("CertificateIDToCancel")]
    public int? CertificateIdToCancel { get; set; }

    [StringLength(35)]
    public string CertificateNumber { get; set; } = null!;

    [Column("CertificateOfOriginStatusID")]
    public int CertificateOfOriginStatusId { get; set; }

    public int? DestinationCountry { get; set; }

    [StringLength(255)]
    public string? FeedbackRemark { get; set; }

    [StringLength(35)]
    public string? InternalApplication { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? IssuingDate { get; set; }

    [StringLength(512)]
    public string? RejectCancelReason { get; set; }

    [StringLength(255)]
    public string? ReplacementReason { get; set; }

    public int RequestReasonCode { get; set; }

    [StringLength(17)]
    public string? ExportDeclarationNumber { get; set; }

    [StringLength(255)]
    public string? CertificateToReplaceInImport { get; set; }

    [Column("GUID")]
    public Guid? Guid { get; set; }

    [Column("QRCodePath")]
    [StringLength(1000)]
    public string? QrCodePath { get; set; }

    public bool IsAttachedList { get; set; }

    public bool? InSufficentworkingInd { get; set; }

    [StringLength(255)]
    public string? InsufficentWorkingText { get; set; }

    public byte[]? QrImage { get; set; }

    [Column("ApproveUserID")]
    public int? ApproveUserId { get; set; }

    public int VersionNumber { get; set; }

    public bool IsLastVersion { get; set; }

    public bool IsInPublishingProcess { get; set; }
}
