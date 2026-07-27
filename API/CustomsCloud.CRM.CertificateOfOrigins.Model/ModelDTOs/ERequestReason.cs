using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Generated from CRM.CertificateOfOrigins_enum_RequestReasonCode. Display/Description texts are the Hebrew
// names from the legacy enum (source of truth — not invented).
[Flags]
public enum ERequestReason
{
    [Display(Name = "הוספת תעודה חדשה", Description = "הוספת תעודה חדשה")]
    [Description("הוספת תעודה חדשה")]
    NewCertificate = 1,

    [Display(Name = "הוספת תעודה בדיעבד", Description = "הוספת תעודה בדיעבד")]
    [Description("הוספת תעודה בדיעבד")]
    RetrospectiveCertificate = 2,

    [Display(Name = "תיקון תעודה", Description = "תיקון תעודה")]
    [Description("תיקון תעודה")]
    CertificateUpdate = 3,

    [Display(Name = "החלפת תעודה", Description = "החלפת תעודה")]
    [Description("החלפת תעודה")]
    CertificateReplacement = 4,

    [Display(Name = "החלפת תעודה ביבוא", Description = "החלפת תעודה ביבוא")]
    [Description("החלפת תעודה ביבוא")]
    ImportCertificateReplacement = 5,

    [Display(Name = "החלטה של וועדת שוק אזורית", Description = "החלטה של וועדת שוק אזורית")]
    [Description("החלטה של וועדת שוק אזורית")]
    ResolutionOfRegionalMarketCommittee = 6,

    [Display(Name = "החלטה של וועדת שוק עליונה", Description = "החלטה של וועדת שוק עליונה")]
    [Description("החלטה של וועדת שוק עליונה")]
    DecisionOfSupremeMarketCommission = 7,

    [Display(Name = "אקראי", Description = "אקראי")]
    [Description("אקראי")]
    Random = 8,

    [Display(Name = "בדיקה מקיפה", Description = "בדיקה מקיפה")]
    [Description("בדיקה מקיפה")]
    ComprehensiveTesting = 9,

    [Display(Name = "תעודה ריקה", Description = "תעודה ריקה")]
    [Description("תעודה ריקה")]
    EmptyCertificate = 10,

    [Display(Name = "טיוטה", Description = "טיוטה")]
    [Description("טיוטה")]
    Draft = 12,

    [Display(Name = "קבלת סטטוס בקשה", Description = "קבלת סטטוס בקשה")]
    [Description("קבלת סטטוס בקשה")]
    GetRequestStatus = 13,

    [Display(Name = "ביטול תעודה", Description = "ביטול תעודה")]
    [Description("ביטול תעודה")]
    CertificateCancellation = 14,
}
