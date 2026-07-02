using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

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

    [StringLength(255)]
    public string? Value { get; set; }

    [StringLength(255)]
    public string? DisplayedValue { get; set; }
}
