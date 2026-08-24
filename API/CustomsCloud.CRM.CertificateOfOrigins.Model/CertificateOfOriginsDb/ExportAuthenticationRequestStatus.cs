using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// Enum table the export-search joins (INNER JOIN on StatusID) to resolve RequestStatusName.
// Added when GetExportDocumentAuthenticationRequestSearch moved from the SP to a LINQ join in the DAL.
[Table("CertificateOfOrigins_enum_ExportAuthenticationRequestStatus", Schema = "CRM")]
public class ExportAuthenticationRequestStatus
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = null!;

    [Column("State")]
    public int State { get; set; }

    [Column("Description")]
    public string? Description { get; set; }

    [Column("EnglishName")]
    public string EnglishName { get; set; } = null!;

    [Column("Enumeration")]
    public string Enumeration { get; set; } = null!;

    [Column("StartDate")]
    public DateTime? StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }
}
