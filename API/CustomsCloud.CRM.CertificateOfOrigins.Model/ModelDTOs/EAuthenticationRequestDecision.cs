namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Import authentication-request decision (CRM.CertificateOfOrigins_enum...Decision). Curated subset — only the
// decisions this service sets. Values are the source of truth from the platform enum (not invented).
public enum EAuthenticationRequestDecision
{
    LetterForImporterWasSent = 8,

    ReminderForImporterWasSent = 9,
}
