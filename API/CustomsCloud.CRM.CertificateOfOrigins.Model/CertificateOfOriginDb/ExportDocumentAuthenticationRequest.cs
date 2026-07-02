using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_ExportDocumentAuthenticationRequest", Schema = "CRM")]
public class ExportDocumentAuthenticationRequest
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(25)]
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

    [Column("AuthenticationDocumentTypeID")]
    public int AuthenticationDocumentTypeId { get; set; }

    [Column("ExporterCustomerID")]
    public int? ExporterCustomerId { get; set; }

    [Column("StatusID")]
    public int? StatusId { get; set; }

    [Column("CountryID")]
    public int? CountryId { get; set; }

    [StringLength(1000)]
    public string? CustomsHouseAddress { get; set; }

    [Column("VendorID")]
    public int? VendorId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AuthenticationRequestArrivalDate { get; set; }

    [StringLength(255)]
    public string? AuthenticationRequestedByName { get; set; }

    [StringLength(255)]
    public string? AuthenticationRequestedByEmail { get; set; }

    [StringLength(14)]
    public string? AuthenticationRequestedByPhone { get; set; }

    [StringLength(255)]
    public string AuthenticationRequestNotes { get; set; } = null!;

    [Column("ExportLeadDocumentID")]
    public int? ExportLeadDocumentId { get; set; }

    [Column("DocumentID")]
    public int? DocumentId { get; set; }

    [StringLength(255)]
    public string? MainDocumentTitle { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastDeliveryDate { get; set; }

    [Column("DeliveryMethodID")]
    public int? DeliveryMethodId { get; set; }

    [StringLength(600)]
    public string? InvoiceNumbers { get; set; }

    [StringLength(1000)]
    public string? DetailedDecision { get; set; }

    [StringLength(255)]
    public string? ReferenceNumber { get; set; }

    [StringLength(1000)]
    public string? CommentForCustomsHouseLetter { get; set; }

    public int? TotalDocuments { get; set; }

    public int? TotalInvoices { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DocumentDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InvoiceDate { get; set; }
}
