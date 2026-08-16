using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail — a certificate's invoice rows. Read-only reconciliation
// (UpdateCertificateOfOrigins) uses InvoiceNumber; the full column set (mapped from the EF4 EDMX) is the write shape the
// GetPC_MSG2280_2281 create branch persists.
[Table("CertificateOfOrigins_CertificateOfOriginInvoiceDetail", Schema = "CRM")]
public class CertificateOfOriginInvoiceDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginID")]
    public int CertificateOfOriginId { get; set; }

    [Column("CurrencyTypeID")]
    public int? CurrencyTypeId { get; set; }

    [Column("InvoiceAmount")]
    public decimal InvoiceAmount { get; set; }

    [Column("InvoiceDate")]
    public DateTime InvoiceDate { get; set; }

    [Column("InvoiceGoodsDescription")]
    public string InvoiceGoodsDescription { get; set; } = null!;

    [Column("InvoiceNumber")]
    public string? InvoiceNumber { get; set; }

    [Column("IsToPrint")]
    public bool IsToPrint { get; set; }
}
