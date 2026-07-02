using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_enum_CertificateDetailsTypeCode", Schema = "CRM")]
public class CertificateDetailsTypeCode
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

    [StringLength(255)]
    public string? Comment { get; set; }

    [StringLength(255)]
    public string? DetailTypeFormat { get; set; }

    [Column("DataTypeID")]
    public int DataTypeId { get; set; }
}
