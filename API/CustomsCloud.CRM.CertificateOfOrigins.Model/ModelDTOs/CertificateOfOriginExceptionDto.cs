namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// One reconciliation error returned by UpdateCertificateOfOrigins (legacy EAISchema.Exception). Describes a mismatch
// between the certificate and the export declaration, with its severity level and the localized + English text.
public class CertificateOfOriginExceptionDto
{
    // EExceptionLevel numeric value (Error / Warning).
    public int? ExceptionLevel { get; set; }

    public string? EnglishDescription { get; set; }

    public string? ExceptionDescription { get; set; }

    // The EMessages code the exception was built from.
    public int ExceptionType { get; set; }
}
