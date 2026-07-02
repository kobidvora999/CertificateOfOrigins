using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

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

    [Column(TypeName = "money")]
    public decimal InvoiceAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime InvoiceDate { get; set; }

    [StringLength(255)]
    public string InvoiceGoodsDescription { get; set; } = null!;

    [StringLength(35)]
    public string InvoiceNumber { get; set; } = null!;

    public bool IsToPrint { get; set; }
}
