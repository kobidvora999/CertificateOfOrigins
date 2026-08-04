namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Curated subset of the platform ETaskType — only the task types this service queries (GetAuthenticationRequestByID).
// Values are the source of truth from the platform enum (not invented).
public enum ETaskType
{
    // The importer-reminder task.
    SendReminderForImporter = 404,

    // The set-decision-before-association task (raised per request when a new file is created).
    SetDecisionBeforeAssociation = 406,

    // The handle-rejected-authentication-request task.
    HandleRejectedAuthenticationRequest = 407,
}
