using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement: the trade agreements each certificate type is
// issued under. Legacy CertificateOfOriginsUtil.GetTradeAgreementsForCertificateType read it for
// ServicesAdapter.IsTradeAgreementForCountry. ValidFrom / ValidTo are carried but legacy never filtered on them.
[Table("CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement", Schema = "CRM")]
public class CertificateOfOriginTypeByTradeAgreement
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginTypeCodeID")]
    public int CertificateOfOriginTypeCodeId { get; set; }

    [Column("TradeAgreementID")]
    public int TradeAgreementId { get; set; }

    [Column("ValidFrom")]
    public DateTime ValidFrom { get; set; }

    [Column("ValidTo")]
    public DateTime ValidTo { get; set; }
}
