using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_enum_AuthenticationFileStatus (EDMX: key = ID). The authentication-file status lookup
// table, read in full by GetAuthenticationRequestFileByID (legacy GetQuery<CertificateOfOriginsAuthenticationFileStatus>
// ().ToList() → file.FileStatuses). Distinct from the curated EAuthenticationFileStatus enum (this carries the rows).
[Table("CertificateOfOrigins_enum_AuthenticationFileStatus", Schema = "CRM")]
public class CertificateOfOriginsAuthenticationFileStatus
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

    [Column("EndDate")]
    public DateTimeOffset? EndDate { get; set; }

    [Column("IsAutomatic")]
    public bool IsAutomatic { get; set; }
}
