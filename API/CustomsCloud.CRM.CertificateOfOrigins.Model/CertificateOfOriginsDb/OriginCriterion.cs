using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_c_OriginCriterion — the origin-criterion C-table (owned by this service, resolved locally).
// The GetPC_MSG2280_2281 create branch resolves an invoice item's OriginCriterion code, scoped to the certificate type,
// to its id (legacy SystemTablesUtil.GetTablesSync<OriginCriterion> with a code + certificate-type predicate).
[Table("CertificateOfOrigins_c_OriginCriterion", Schema = "CRM")]
public class OriginCriterion
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("OriginCriterionCode")]
    public string OriginCriterionCode { get; set; } = null!;

    [Column("CertificateOfOriginTypeCodeID")]
    public int CertificateOfOriginTypeCodeId { get; set; }

    [Column("EnglishName")]
    public string EnglishName { get; set; } = null!;
}
