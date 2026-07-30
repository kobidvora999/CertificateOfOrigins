namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Curated subset of the platform EEventType (MalamTeam...Environment.Enums.EEventType) — only the event-type ids
// this service raises. Values are the source of truth from the platform enum (not invented). Add a new member
// here (with its real numeric value) instead of scattering event-type literals across the code.
public enum EEventType
{
    // Closes all open tasks for an import authentication-request file. The Events microservice's response handler
    // performs the task closure; the caller only raises the event.
    CloseAllTaskForImportAuthenticationRequestFile = 1525,

    // Closes the 3-month reminder-notice task for an import authentication-request file (raised by CloseReminderTask).
    CloseTaskReminderNotice3Months = 1745,

    // A delivery / reminder letter was sent to the importer (raised by the importer delivery/reminder flow).
    NewDeliveryForImporterSent = 1511,

    NewDeliveryReminderForImporterSent = 1512,
}
