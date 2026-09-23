namespace CertificateOfOrigins.Model.ModelDTOs;

// Import authentication-request file status (CRM.CertificateOfOrigins_enum_AuthenticationFileStatus). Values are
// the source of truth from the platform enum (not invented).
public enum EAuthenticationFileStatus
{
    WaitingForSendingLetter = 1,
    AuthenticationRequestWasSend = 2,
    AuthenticationRequestReminderWasSend = 3,
    ReceivedPartialAnswerInFile = 4,
    ReceivedAnswerInFile = 5,
    RightAuthenticationAnswer = 6,
    ClarificationRequired = 7,
    WrongAuthenticationAnswer = 8,
    CancelledFile = 9,

    // CR 194221 — a customs worker closes the file administratively. Carries the same side effects as
    // RightAuthenticationAnswer ("תיק תקין"): the collaterals are released and the handling task is opened.
    AdministrativeClosure = 10,
}
