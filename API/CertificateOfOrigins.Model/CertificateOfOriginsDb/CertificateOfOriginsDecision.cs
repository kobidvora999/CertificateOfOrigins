using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertificateOfOrigins.Model.CertificateOfOriginsDb;

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

    // Audience flags — which screen's decision dropdown the value belongs to. The legacy WPF client filters on them
    // (AuthenticationRequestFilePresenter: Decisions.Where(d => d.IsForClaliMakorWorker); ImportProcessFormPresenter:
    // Decisions.Where(d => d.IsForCoordinator)). Onboarded for CR 194221 so an API consumer can filter the same way —
    // the file-status lookup already exposes its own IsAutomatic flag.
    [Column("IsForCoordinator")]
    public bool IsForCoordinator { get; set; }

    [Column("IsForClaliMakorWorker")]
    public bool IsForClaliMakorWorker { get; set; }
}
