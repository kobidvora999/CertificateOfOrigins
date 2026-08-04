using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_ItemDetails (EDMX: key = Id identity). The item lines of an import authentication
// request (SP result-set #2 of GetImportAuthenticationRequestById), read by ImportAuthenticationRequestID.
[Table("CertificateOfOrigins_ItemDetails", Schema = "CRM")]
public class CertificateOfOriginsItemDetails
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("ImportAuthenticationRequestID")]
    public int? ImportAuthenticationRequestId { get; set; }

    [Column("CustomItemID")]
    public int CustomItemId { get; set; }
}
