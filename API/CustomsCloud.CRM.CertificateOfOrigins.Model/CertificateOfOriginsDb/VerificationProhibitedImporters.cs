using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

[Table("CertificateOfOrigins_c_VerificationProhibitedImporters", Schema = "CRM")]
public class VerificationProhibitedImporters
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int CustomerId { get; set; }
}
