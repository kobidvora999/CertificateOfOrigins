using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_CertificateOfOriginDetails — the certificate's detail rows (one per field: country /
// trade-agreement / date / site …). Onboarded for SaveCertificateOfOrigin, which diff-merges this child collection
// by surrogate Id. Reads elsewhere use the SP; this entity is for the write path.
[Table("CertificateOfOrigins_CertificateOfOriginDetails", Schema = "CRM")]
public class CertificateOfOriginDetails
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginID")]
    public int CertificateOfOriginId { get; set; }

    [Column("CertificateDetailsTypeCodeID")]
    public int CertificateDetailsTypeCodeId { get; set; }

    public string? Value { get; set; }

    public string? DisplayedValue { get; set; }
}
