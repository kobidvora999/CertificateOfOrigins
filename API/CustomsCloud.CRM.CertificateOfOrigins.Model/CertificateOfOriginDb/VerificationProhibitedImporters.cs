using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_c_VerificationProhibitedImporters", Schema = "CRM")]
public class VerificationProhibitedImporters
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CustomerId")]
    public int CustomerId { get; set; }
}
