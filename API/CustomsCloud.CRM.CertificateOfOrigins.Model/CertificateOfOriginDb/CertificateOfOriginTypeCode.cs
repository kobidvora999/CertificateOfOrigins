using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_enum_CertificateOfOriginTypeCode", Schema = "CRM")]
public class CertificateOfOriginTypeCode
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int State { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string EnglishName { get; set; } = null!;

    [StringLength(255)]
    public string Enumeration { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    public bool IsCriterionMandatory { get; set; }

    public bool IsCustomApprovalRequired { get; set; }

    public int? ReportId { get; set; }

    public bool? IsCustomsItemMandatory { get; set; }

    public bool IsZipcodeMandatory { get; set; }
}
