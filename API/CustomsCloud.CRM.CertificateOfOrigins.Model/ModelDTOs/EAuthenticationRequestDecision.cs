namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Import authentication-request decision (CRM.CertificateOfOrigins_enum_Decision). Curated subset — only the decisions
// this service sets. Values are the source of truth from the platform enum / the enum_Decision table (not invented).
public enum EAuthenticationRequestDecision
{
    NewAuthenticationRequest = 1,

    Rejection = 2,

    Approval = 3,

    DemandAnotherClarification = 4,

    Partly = 5,

    AuthenticationRequried = 6,

    AuthenticationNeedless = 7,

    LetterForImporterWasSent = 8,

    ReminderForImporterWasSent = 9,
}
