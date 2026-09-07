namespace CertificateOfOrigins.Model.ModelDTOs;

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

    // Closes the SetDecisionBeforeAssociation task for a request (raised per request when a new file is created).
    NewDecisionBeforeAssociation = 1515,

    // Opens the HandleAuthenticationRequestFile task for a newly created authentication-request file.
    NewAuthenticationRequestFile = 1517,

    // Export-document authentication-request status flow (SaveExportDocumentAuthenticationRequest). The generic
    // status-update event is always raised; the others are raised per the target status.
    ExportAuthenticationRequestFileStatusUpdate = 1282,

    ExportNewAuthenticationRequest = 1307,

    ExportAuthenticationRequestAfterClosing = 1617,

    ChangeFileStatus = 2047,

    // Import authentication-request decision flow (SaveImportAuthenticationRequest). NewAuthenticationRequest opens the
    // SetDecisionBeforeAssociation task; AuthenticationRequestRejected opens the rejection task (assigned to the
    // responder); ImportAuthenticationRequestProcessedWithWasRejected marks a re-processed rejected request.
    NewAuthenticationRequest = 1283,

    AuthenticationRequestRejected = 1516,

    ImportAuthenticationRequestProcessedWithWasRejected = 1990,

    // Import authentication-request FILE save flow (SaveAuthenticationRequestFile). Per changed request:
    // CloseAllTaskForImportAuthenticationRequest closes its open tasks, AuthenticationRequestDecisionUpdate logs the
    // decision change. Per file-status change: AuthenticationRequestFileStatusUpdate logs it, HandleImportAuthenticationRequest
    // opens the handling task, and the two UpdateFileStatus* events fire on the specific target statuses.
    AuthenticationRequestFileStatusUpdate = 1281,

    AuthenticationRequestDecisionUpdate = 1472,

    CloseAllTaskForImportAuthenticationRequest = 1524,

    HandleImportAuthenticationRequest = 1641,

    UpdateFileStatusVendorReminderNotice = 1906,

    UpdateFileStatusFinalDecisionInCase = 1915,

    // Certificate-of-origin save flow (SaveCertificateOfOrigin) — application received/corrected, user deny/approve/
    // cancel, certificate issued/replaced, and the declaration match/mismatch + new-certificate events. Values are
    // the source of truth from the platform enum (verified 2026-08-06).
    CertificateOfOriginNewCertificateOfOriginCreated = 599,

    CertificateOfOriginApplicationReceived = 609,

    CertificateOfOriginApplicationCorrected = 610,

    CertificateOfOriginUserDeniedCertificate = 636,

    CertificateOfOriginUserApprovedCertificate = 637,

    CertificateOfOriginUserCancelledCertificate = 638,

    CertificateOfOriginCertificateReplaced = 639,

    CertificateOfOriginCertificateIssued = 640,

    CertificateOfOriginCertificateMatchDeclaration = 642,

    CertificateOfOriginCertificateDeclarationMismatch = 643,

    CertificateMatchDeclarationWithoutTask = 1913,

    NewCertificateOfOriginCreatedWithoutTask = 1914,

    // Certificate-vs-declaration reconciliation (UpdateCertificateOfOrigins): declaration-has-warnings (mismatch with
    // warnings) + open the import-certificate-replacement handling task.
    CertificateOfOriginCertificateDeclarationHasWarnings = 2171,

    OpenTaskHandlingTheReplacementOfAnImportCertificate = 2172,

    // --- Reminder schedulers (the two C17 Planar jobs) ---

    // Raised per import-authentication request whose importer letter has gone unanswered past the reminder window;
    // opens the SendReminderForImporter task. Source: ReminderForImporterScheduler.
    NewReminderForImporterCreated = 1510,

    // The reminder ladder raised by AuthenticationRequestReminder. Which member fires depends on the delivery method
    // together with the IsImport / IsVendor / SendThreeMonthsReminder flags the scheduler SP returns per row.
    ReminderNotice3Months = 1538,

    ReminderNotice6Months = 1275,

    ImporterReminderNotice6Months = 1907,

    ExportReminderNotice6Months = 1305,

    ReminderNotice10Months = 1276,

    ExportReminderNotice10Months = 1306,

    FinalDecisionInTheFile = 1908,
}
