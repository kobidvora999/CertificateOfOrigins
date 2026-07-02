using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument", Schema = "CRM")]
public class ExportDocumentAuthenticationRequestLeadDocument
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ExportRequestID")]
    public int ExportRequestId { get; set; }

    [Column("LeadDocumentID")]
    public int? LeadDocumentId { get; set; }

    [StringLength(14)]
    public string LeadDocumentTitle { get; set; } = null!;
}
