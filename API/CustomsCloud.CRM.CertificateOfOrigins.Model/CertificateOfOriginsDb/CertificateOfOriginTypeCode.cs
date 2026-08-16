using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode — the certificate-type C-table. Read by
// UpdateCertificateOfOrigins reconciliation (IsCustomsItemMandatory) and by the GetPC_MSG2280_2281 create branch, which
// reads all three mandatory flags to drive origin-criterion / customs-item / zipcode validation.
[Table("CertificateOfOrigins_enum_CertificateOfOriginTypeCode", Schema = "CRM")]
public class CertificateOfOriginTypeCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IsCriterionMandatory")]
    public bool IsCriterionMandatory { get; set; }

    // bit NULL in the C-table — null means "not mandatory" for the reconciliation gate.
    [Column("IsCustomsItemMandatory")]
    public bool? IsCustomsItemMandatory { get; set; }

    [Column("IsZipcodeMandatory")]
    public bool IsZipcodeMandatory { get; set; }
}
