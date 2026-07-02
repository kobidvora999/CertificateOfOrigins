using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_CertificateOfOriginVsDeclarationError", Schema = "CRM")]
public class CertificateOfOriginVsDeclarationError
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginID")]
    public int CertificateOfOriginId { get; set; }

    [StringLength(512)]
    public string? ErrorText { get; set; }

    public int State { get; set; }
}
