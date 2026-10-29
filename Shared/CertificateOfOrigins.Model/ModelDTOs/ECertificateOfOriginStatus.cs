using System.ComponentModel.DataAnnotations;

namespace CertificateOfOrigins.Model.ModelDTOs;

// Certificate-of-origin status (CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode). Values are the source of
// truth from the DB C-table (verified 2026-08-06). Drives the SaveCertificateOfOrigin / UpdateCertificateOfOrigins
// status machine (cancel / validate / declaration match-mismatch / publish). The Display names are the Hebrew `Name`
// column of the C-table, used where legacy put the table Name into a message parameter.
public enum ECertificateOfOriginStatus
{
    [Display(Name = "שגויה")]
    Error = 1,

    [Display(Name = "נקלטה")]
    Received = 2,

    [Display(Name = "נדחתה")]
    Rejected = 3,

    [Display(Name = "מבוטלת")]
    Cancelled = 4,

    [Display(Name = "אינה תואמת להצהרת יצוא")]
    DeclarationMismatch = 5,

    [Display(Name = "תואמת להצהרת יצוא")]
    DeclarationMatch = 6,

    [Display(Name = "ממתינה להתרת הצהרת יצוא")]
    PendingRelease = 7,

    [Display(Name = "מאושרת לפרסום באינטרנט")]
    Published = 8,
}
