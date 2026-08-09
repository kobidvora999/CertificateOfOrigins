using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode — the certificate-type C-table. Onboarded (read-only) for
// UpdateCertificateOfOrigins reconciliation, which reads IsCustomsItemMandatory to decide whether the customs-item
// (6-digit classification) match applies. Only the columns the reconciler reads are mapped.
[Table("CertificateOfOrigins_enum_CertificateOfOriginTypeCode", Schema = "CRM")]
public class CertificateOfOriginTypeCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    // bit NULL in the C-table — null means "not mandatory" for the reconciliation gate.
    [Column("IsCustomsItemMandatory")]
    public bool? IsCustomsItemMandatory { get; set; }
}
