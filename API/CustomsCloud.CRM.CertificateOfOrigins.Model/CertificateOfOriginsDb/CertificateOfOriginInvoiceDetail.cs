using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail — a certificate's invoice rows. Onboarded (read-only) for
// UpdateCertificateOfOrigins reconciliation, which matches each certificate invoice number against the export
// declaration's invoices. Only the columns the reconciler reads are mapped.
[Table("CertificateOfOrigins_CertificateOfOriginInvoiceDetail", Schema = "CRM")]
public class CertificateOfOriginInvoiceDetail
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginID")]
    public int CertificateOfOriginId { get; set; }

    [Column("InvoiceNumber")]
    public string? InvoiceNumber { get; set; }
}
