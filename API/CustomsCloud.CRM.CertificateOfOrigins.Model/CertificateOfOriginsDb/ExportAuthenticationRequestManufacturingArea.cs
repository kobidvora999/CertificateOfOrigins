using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

[Table("CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea", Schema = "CRM")]
public class ExportAuthenticationRequestManufacturingArea
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ExportAuthenticationRequestID")]
    public int ExportAuthenticationRequestId { get; set; }

    [StringLength(255)]
    public string? ManufacturingArea { get; set; }

    [StringLength(255)]
    public string? ManufacturingZipcode { get; set; }

    [ForeignKey(nameof(ExportAuthenticationRequestId))]
    public ExportDocumentAuthenticationRequest? Request { get; set; }
}
