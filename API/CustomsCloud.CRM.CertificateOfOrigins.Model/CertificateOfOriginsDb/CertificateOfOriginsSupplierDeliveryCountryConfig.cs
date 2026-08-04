using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig (EDMX: no real PK — ID inferred). Config of the
// countries treated as supplier-delivery (vendor) countries. IsVendorByIssuingCountryID checks whether the issuing
// country has a row here (legacy GetIdByCode<...>("ConutryID", id) > 0). The column-name typo "ConutryID" is
// verbatim from the DB.
[Table("CertificateOfOrigins_cf_SupplierDeliveryCountryConfig", Schema = "CRM")]
public class CertificateOfOriginsSupplierDeliveryCountryConfig
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ConutryID")]
    public int ConutryId { get; set; }

    [Column("State")]
    public int State { get; set; }
}
