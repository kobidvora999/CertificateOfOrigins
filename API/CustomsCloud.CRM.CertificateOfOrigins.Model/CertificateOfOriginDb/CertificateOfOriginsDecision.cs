using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_enum_Decision", Schema = "CRM")]
public class CertificateOfOriginsDecision
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

    public bool IsAutomatic { get; set; }

    public bool IsForCoordinator { get; set; }

    public bool IsForClaliMakorWorker { get; set; }
}
