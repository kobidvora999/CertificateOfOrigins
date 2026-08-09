using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginItemDetail — the goods-item rows under a certificate invoice. Onboarded
// (read-only) for UpdateCertificateOfOrigins reconciliation, which matches each goods item's customs item against the
// export declaration. Only the columns the reconciler reads are mapped.
[Table("CertificateOfOrigins_CertificateOfOriginItemDetail", Schema = "CRM")]
public class CertificateOfOriginItemDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginInvoiceDetailID")]
    public int CertificateOfOriginInvoiceDetailId { get; set; }

    [Column("CustomsItemID")]
    public int? CustomsItemId { get; set; }
}
