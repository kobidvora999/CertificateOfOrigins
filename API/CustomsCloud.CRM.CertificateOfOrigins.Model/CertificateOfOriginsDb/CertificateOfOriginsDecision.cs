using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_enum_Decision (EDMX: key = ID). The decision lookup table, read in full by
// GetAuthenticationRequestByID (legacy GetQuery<CertificateOfOriginsDecision>().ToList()).
[Table("CertificateOfOrigins_enum_Decision", Schema = "CRM")]
public class CertificateOfOriginsDecision
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Name")]
    public string? Name { get; set; }

    [Column("State")]
    public int State { get; set; }

    [Column("Description")]
    public string? Description { get; set; }

    [Column("EnglishName")]
    public string? EnglishName { get; set; }

    [Column("Enumeration")]
    public string? Enumeration { get; set; }

    [Column("StartDate")]
    public DateTimeOffset? StartDate { get; set; }
}
