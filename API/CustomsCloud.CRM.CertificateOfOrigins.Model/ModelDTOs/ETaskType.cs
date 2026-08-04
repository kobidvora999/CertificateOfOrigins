namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Curated subset of the platform ETaskType — only the task types this service queries (GetAuthenticationRequestByID).
// Values are the source of truth from the platform enum (not invented).
public enum ETaskType
{
    // The 6-month / 10-month reminder-notice tasks + the handle-authentication-file task (queried for the file's
    // IsCurrentUserHandleFile flag in GetAuthenticationRequestFileByID).
    ReminderNotice6Months = 339,

    ReminderNotice10Months = 340,

    HandleAuthenticationRequestFile = 408,

    // The importer-reminder task.
    SendReminderForImporter = 404,

    // The set-decision-before-association task (raised per request when a new file is created).
    SetDecisionBeforeAssociation = 406,

    // The handle-rejected-authentication-request task.
    HandleRejectedAuthenticationRequest = 407,
}
