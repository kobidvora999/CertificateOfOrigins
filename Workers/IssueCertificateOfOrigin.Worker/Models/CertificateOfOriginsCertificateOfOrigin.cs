using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IssueCertificateOfOrigin.Worker.Models;

[Table("CertificateOfOrigins_CertificateOfOrigin", Schema = "CRM")]
public partial class CertificateOfOriginsCertificateOfOrigin
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public int State { get; set; }

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
    public int? CertificateIdtoCancel { get; set; }

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
    public string? QrcodePath { get; set; }

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

    [ForeignKey("CertificateIdtoCancel")]
    [InverseProperty("InverseCertificateIdtoCancelNavigation")]
    public virtual CertificateOfOriginsCertificateOfOrigin? CertificateIdtoCancelNavigation { get; set; }

    [InverseProperty("CertificateIdtoCancelNavigation")]
    public virtual ICollection<CertificateOfOriginsCertificateOfOrigin> InverseCertificateIdtoCancelNavigation { get; set; } = new List<CertificateOfOriginsCertificateOfOrigin>();
}
