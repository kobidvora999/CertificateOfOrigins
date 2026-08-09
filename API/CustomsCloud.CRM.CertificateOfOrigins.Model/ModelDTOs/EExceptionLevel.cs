namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Severity of a reconciliation exception (legacy Customs.CRM.CertificateOfOrigins.InternalCommon EExceptionLevel).
// A null level is treated as Error (legacy convention: hasErrors = !level.HasValue || level == Error).
public enum EExceptionLevel
{
    Error = 1,

    Warning = 2,
}
