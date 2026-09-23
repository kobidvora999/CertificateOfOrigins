namespace CertificateOfOrigins.Model.ModelDTOs;

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

    // CR 194221 — a customs worker closes the request administratively. Behaves like every other manual decision
    // (close the open tasks, send the decision message, log the decision event); unlike Approval it does NOT grant
    // the collaterals (confirmed with the analyst, 2026-09-22).
    AdministrativeClosure = 10,
}
