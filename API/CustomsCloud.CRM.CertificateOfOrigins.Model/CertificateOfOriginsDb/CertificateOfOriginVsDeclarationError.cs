using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginVsDeclarationError — a reconciliation-mismatch log row for a certificate
// (UpdateCertificateOfOrigins). Append-only: one row per mismatch error text.
[Table("CertificateOfOrigins_CertificateOfOriginVsDeclarationError", Schema = "CRM")]
public class CertificateOfOriginVsDeclarationError
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginID")]
    public int CertificateOfOriginId { get; set; }

    public string? ErrorText { get; set; }

    public int State { get; set; }
}
