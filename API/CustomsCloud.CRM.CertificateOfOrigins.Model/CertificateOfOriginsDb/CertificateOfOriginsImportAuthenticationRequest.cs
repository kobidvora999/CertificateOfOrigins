using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_ImportAuthenticationRequest (EDMX: key = DocumentID). Onboarded for GetEntityDocuments,
// which reads it by LeadDocumentID (and by DocumentID). The DAL projects only the columns it needs; the full
// column set is declared for completeness.
[Table("CertificateOfOrigins_ImportAuthenticationRequest", Schema = "CRM")]
public class CertificateOfOriginsImportAuthenticationRequest
{
    [Key]
    [Column("DocumentID")]
    public int DocumentId { get; set; }

    [Column("CreateDate")]
    public DateTimeOffset CreateDate { get; set; }

    [Column("CreateUserID")]
    public int CreateUserId { get; set; }

    [Column("UpdateDate")]
    public DateTimeOffset UpdateDate { get; set; }

    [Column("UpdateUserID")]
    public int UpdateUserId { get; set; }

    [Column("AuthenticationFileID")]
    public int? AuthenticationFileId { get; set; }

    [Column("AuthenticationRequestDate")]
    public DateTimeOffset AuthenticationRequestDate { get; set; }

    [Column("CirumstanceDetails")]
    public string? CirumstanceDetails { get; set; }

    [Column("CollateralID")]
    public int? CollateralId { get; set; }

    [Column("DecisionCircumstences")]
    public string? DecisionCircumstences { get; set; }

    [Column("DecisionID")]
    public int? DecisionId { get; set; }

    [Column("LeadDocumentID")]
    public int LeadDocumentId { get; set; }

    [Column("DocumentIssuingDate")]
    public DateTimeOffset DocumentIssuingDate { get; set; }

    [Column("ImportCountryID")]
    public int ImportCountryId { get; set; }

    [Column("IssuingCountryID")]
    public int IssuingCountryId { get; set; }

    [Column("ItemDetailID")]
    public int ItemDetailId { get; set; }

    [Column("Number")]
    public int Number { get; set; }

    [Column("IsOldIndication")]
    public bool IsOldIndication { get; set; }

    [Column("OriginCountryID")]
    public int OriginCountryId { get; set; }

    [Column("PreferenceDocumentTypeID")]
    public int PreferenceDocumentTypeId { get; set; }

    [Column("Remarks")]
    public string? Remarks { get; set; }

    [Column("RequestCircumstancesID")]
    public int RequestCircumstancesId { get; set; }

    [Column("UserResponseID")]
    public int UserResponseId { get; set; }

    [Column("ResponseNameEmail")]
    public string? ResponseNameEmail { get; set; }

    [Column("ResponsePhoneNum")]
    public string? ResponsePhoneNum { get; set; }

    [Column("OrganizationUnitID")]
    public int OrganizationUnitId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("VendorId")]
    public int? VendorId { get; set; }

    [Column("VendorName")]
    public string? VendorName { get; set; }

    [Column("OrganizationUnitTypeID")]
    public int? OrganizationUnitTypeId { get; set; }

    [Column("DocumentNumber")]
    public string? DocumentNumber { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("ImporterID")]
    public int? ImporterId { get; set; }

    [Column("LastDeliveryForImporter")]
    public DateTimeOffset? LastDeliveryForImporter { get; set; }

    [Column("InvoiceNumber")]
    public string? InvoiceNumber { get; set; }

    [Column("InvoiceGoodsItemTaxDifference")]
    public decimal? InvoiceGoodsItemTaxDifference { get; set; }

    [Column("AllInvoiceGoodsItemTaxDifference")]
    public decimal? AllInvoiceGoodsItemTaxDifference { get; set; }
}
