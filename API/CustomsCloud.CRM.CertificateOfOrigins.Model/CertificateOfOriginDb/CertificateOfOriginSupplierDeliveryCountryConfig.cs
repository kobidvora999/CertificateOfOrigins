using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_cf_SupplierDeliveryCountryConfig", Schema = "CRM")]
public class CertificateOfOriginSupplierDeliveryCountryConfig
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ConutryID")]
    public int ConutryId { get; set; }

    public int State { get; set; }
}
