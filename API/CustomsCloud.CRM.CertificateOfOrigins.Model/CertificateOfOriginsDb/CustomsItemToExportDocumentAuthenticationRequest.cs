using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

[Table("CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest", Schema = "CRM")]
public class CustomsItemToExportDocumentAuthenticationRequest
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ExportDocumentAuthenticationRequestID")]
    public int ExportDocumentAuthenticationRequestId { get; set; }

    [Column("CustomsItemID")]
    public int CustomsItemId { get; set; }

    [ForeignKey(nameof(ExportDocumentAuthenticationRequestId))]
    public ExportDocumentAuthenticationRequest? Request { get; set; }
}
