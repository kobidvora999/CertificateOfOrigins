namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Per-field constraint of a certificate-type detail (legacy CertificateOfOrigins_enum_ConstraintTypeEnum). Drives the
// incoming-message field validation: a blank field is an error only when its constraint is Mandatory.
public enum EConstraintType
{
    Mandatory = 1,

    Optional = 2,

    Condition = 3,
}
