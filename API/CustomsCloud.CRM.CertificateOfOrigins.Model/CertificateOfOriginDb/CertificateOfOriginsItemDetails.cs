using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_ItemDetails", Schema = "CRM")]
public class CertificateOfOriginsItemDetails
{
    [Key]
    public int Id { get; set; }

    [Column("ImportAuthenticationRequestID")]
    public int? ImportAuthenticationRequestId { get; set; }

    [Column("CustomItemID")]
    public int CustomItemId { get; set; }
}
