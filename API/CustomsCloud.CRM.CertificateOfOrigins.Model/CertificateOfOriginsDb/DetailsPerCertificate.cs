using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_cl_DetailsPerCertificate — the per-certificate-type field catalogue: for each certificate
// type, which detail fields (CertificateDetailsTypeCodeID) are relevant and whether each is Mandatory/Optional/Condition
// (ConstraintTypeEnumID). Drives the incoming-message field-validation engine (GetPC_MSG2280_2281): a field is validated
// only if it appears here for the certificate type, and "blank" is an error only when its constraint is Mandatory.
[Table("CertificateOfOrigins_cl_DetailsPerCertificate", Schema = "CRM")]
public class DetailsPerCertificate
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CertificateOfOriginTypeCodeID")]
    public int CertificateOfOriginTypeCodeId { get; set; }

    [Column("ConstraintTypeEnumID")]
    public int ConstraintTypeEnumId { get; set; }

    [Column("CertificateDetailsTypeCodeID")]
    public int CertificateDetailsTypeCodeId { get; set; }

    [Column("Order")]
    public int Order { get; set; }
}
