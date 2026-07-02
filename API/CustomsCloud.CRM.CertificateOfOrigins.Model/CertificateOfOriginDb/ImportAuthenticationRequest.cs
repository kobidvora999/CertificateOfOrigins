using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_ImportAuthenticationRequest", Schema = "CRM")]
public class ImportAuthenticationRequest
{
    [Key]
    [Column("DocumentID")]
    public int DocumentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreateDate { get; set; }

    [Column("CreateUserID")]
    public int CreateUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdateDate { get; set; }

    [Column("UpdateUserID")]
    public int UpdateUserId { get; set; }

    [Column("AuthenticationFileID")]
    public int? AuthenticationFileId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AuthenticationRequestDate { get; set; }

    [StringLength(512)]
    public string? CirumstanceDetails { get; set; }

    [Column("CollateralID")]
    public int? CollateralId { get; set; }

    [StringLength(512)]
    public string? DecisionCircumstences { get; set; }

    [Column("DecisionID")]
    public int? DecisionId { get; set; }

    [Column("LeadDocumentID")]
    public int LeadDocumentId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DocumentIssuingDate { get; set; }

    [Column("ImportCountryID")]
    public int ImportCountryId { get; set; }

    [Column("IssuingCountryID")]
    public int IssuingCountryId { get; set; }

    [Column("ItemDetailID")]
    public int ItemDetailId { get; set; }

    public int Number { get; set; }

    public bool IsOldIndication { get; set; }

    [Column("OriginCountryID")]
    public int OriginCountryId { get; set; }

    [Column("PreferenceDocumentTypeID")]
    public int PreferenceDocumentTypeId { get; set; }

    [StringLength(512)]
    public string Remarks { get; set; } = null!;

    [Column("RequestCircumstancesID")]
    public int RequestCircumstancesId { get; set; }

    [Column("UserResponseID")]
    public int UserResponseId { get; set; }

    [StringLength(255)]
    public string? ResponseNameEmail { get; set; }

    [StringLength(255)]
    public string? ResponsePhoneNum { get; set; }

    [Column("OrganizationUnitID")]
    public int OrganizationUnitId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    public int? VendorId { get; set; }

    [StringLength(45)]
    public string? VendorName { get; set; }

    [Column("OrganizationUnitTypeID")]
    public int? OrganizationUnitTypeId { get; set; }

    [StringLength(255)]
    public string? DocumentNumber { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("ImporterID")]
    public int? ImporterId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastDeliveryForImporter { get; set; }

    [StringLength(225)]
    public string? InvoiceNumber { get; set; }

    [Column(TypeName = "money")]
    public decimal? InvoiceGoodsItemTaxDifference { get; set; }

    [Column(TypeName = "money")]
    public decimal? AllInvoiceGoodsItemTaxDifference { get; set; }
}
