using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Generated from CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus. Values + Hebrew names are the
// source of truth (not invented). The Display name is used for the status-update event / message text (legacy read
// it from SystemTablesUtil.GetCodeById<ExportAuthenticationRequestStatus>.Name — no ILookupUtil type exists for it,
// so the enum Display name is used instead, consistent with #26).
public enum EExportAuthenticationRequestStatus
{
    [Display(Name = "ממתין לשליחת מכתב")]
    [Description("ממתין לשליחת מכתב")]
    WaitingForLetterSending = 1,

    [Display(Name = "ממתין למענה יצואן")]
    [Description("ממתין למענה יצואן")]
    WaitingForExporter = 2,

    [Display(Name = "ממתין למענה יצואן לאחר התראה")]
    [Description("ממתין למענה יצואן לאחר התראה")]
    WaitingForExporterAnswerAfterNotification = 3,

    [Display(Name = "ממתין לפרטים נוספים")]
    [Description("ממתין לפרטים נוספים")]
    WaitingForAdditionalInformation = 4,

    [Display(Name = "מוכן לטיפול מקצועי")]
    [Description("מוכן לטיפול מקצועי")]
    ReadyForProfessionalTreatment = 5,

    [Display(Name = "סגור - תקין")]
    [Description("סגור - תקין")]
    ClosedValid = 6,

    [Display(Name = "סגור - לא תקין")]
    [Description("סגור - לא תקין")]
    ClosedNotValid = 7,

    [Display(Name = "סגור - תקין חלקי")]
    [Description("סגור - תקין חלקי")]
    ClosedSemiValid = 8,

    [Display(Name = "בוטל")]
    [Description("בוטל")]
    Cancelled = 9,
}
