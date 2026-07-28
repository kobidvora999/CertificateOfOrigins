using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Generated from CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode. Display/Description texts are the
// Hebrew names from the legacy enum (source of truth — not invented).
[Flags]
public enum ECertificateDetailsType
{
    [Display(Name = "מס' היצואן", Description = "מס' היצואן")]
    [Description("מס' היצואן")]
    ExporterId = 1,

    [Display(Name = "שם יצואן", Description = "שם יצואן")]
    [Description("שם יצואן")]
    ExporterName = 2,

    [Display(Name = "כתובת יצואן", Description = "כתובת יצואן")]
    [Description("כתובת יצואן")]
    ExporterAddress = 3,

    [Display(Name = "מדינת יצואן", Description = "מדינת יצואן")]
    [Description("מדינת יצואן")]
    ExporterCountry = 4,

    [Display(Name = "מדינה ראשונה בהסכם", Description = "מדינה ראשונה בהסכם")]
    [Description("מדינה ראשונה בהסכם")]
    TradeAgreementCountry1 = 5,

    [Display(Name = "מדינה שניה בהסכם", Description = "מדינה שניה בהסכם")]
    [Description("מדינה שניה בהסכם")]
    TradeAgreementCountry2 = 6,

    [Display(Name = "קבוצת מדינות בהסכם", Description = "קבוצת מדינות בהסכם")]
    [Description("קבוצת מדינות בהסכם")]
    TradeAgreementGroupOfCountries = 7,

    [Display(Name = "שם הנשגר/יבואן", Description = "שם הנשגר/יבואן")]
    [Description("שם הנשגר/יבואן")]
    ConsigneeName = 8,

    [Display(Name = "כתובת הנשגר/יבואן", Description = "כתובת הנשגר/יבואן")]
    [Description("כתובת הנשגר/יבואן")]
    ConsigneeAddress = 9,

    [Display(Name = "מדינת הנשגר/יבואן", Description = "מדינת הנשגר/יבואן")]
    [Description("מדינת הנשגר/יבואן")]
    ConsigneeCountry = 10,

    [Display(Name = "הערות לגבי נשגר/ יבואן", Description = "הערות לגבי נשגר/ יבואן")]
    [Description("הערות לגבי נשגר/ יבואן")]
    ConsigneeRemarks = 11,

    [Display(Name = "האם להדפיס נתוני נשגר/יבואן?", Description = "האם להדפיס נתוני נשגר/יבואן?")]
    [Description("האם להדפיס נתוני נשגר/יבואן?")]
    IsConsigneeForPrint = 12,

    [Display(Name = "מדינת המקור", Description = "מדינת המקור")]
    [Description("מדינת המקור")]
    OriginCountry = 13,

    [Display(Name = "קבוצת מדינות המקור", Description = "קבוצת מדינות המקור")]
    [Description("קבוצת מדינות המקור")]
    OriginGroupOfCountries = 14,

    [Display(Name = "מדינת היעד", Description = "מדינת היעד")]
    [Description("מדינת היעד")]
    DestinationCountry = 15,

    [Display(Name = "קבוצת מדינות היעד", Description = "קבוצת מדינות היעד")]
    [Description("קבוצת מדינות היעד")]
    DestinationGroupOfCountries = 16,

    [Display(Name = "אמצעי הובלה", Description = "אמצעי הובלה")]
    [Description("אמצעי הובלה")]
    Transport = 17,

    [Display(Name = "נמל מוצא", Description = "נמל מוצא")]
    [Description("נמל מוצא")]
    PortOfShipment = 18,

    [Display(Name = "צבירה", Description = "צבירה")]
    [Description("צבירה")]
    IsCumulation = 19,

    [Display(Name = "מדינת צבירה", Description = "מדינת צבירה")]
    [Description("מדינת צבירה")]
    CumulationCountry = 20,

    [Display(Name = "קבוצת מדינות צבירה", Description = "קבוצת מדינות צבירה")]
    [Description("קבוצת מדינות צבירה")]
    CumulationGroupOfCountries = 21,

    [Display(Name = "מקום ייצור הטובין", Description = "מקום ייצור הטובין")]
    [Description("מקום ייצור הטובין")]
    PlaceOfManufacture = 22,

    [Display(Name = "מיקוד של ייצור הטובין", Description = "מיקוד של ייצור הטובין")]
    [Description("מיקוד של ייצור הטובין")]
    ZipCodeOfManufacture = 23,

    [Display(Name = "הערות", Description = "הערות")]
    [Description("הערות")]
    Observations = 24,

    [Display(Name = "הערות מעריך", Description = "הערות מעריך")]
    [Description("הערות מעריך")]
    Feedback = 25,

    [Display(Name = "האם להדפיס את מספר הצהרת היצוא?", Description = "האם להדפיס את מספר הצהרת היצוא?")]
    [Description("האם להדפיס את מספר הצהרת היצוא?")]
    IsExportDecForPrint = 26,

    [Display(Name = "בית מכס", Description = "בית מכס")]
    [Description("בית מכס")]
    CustomsHouse = 27,

    [Display(Name = "מדינה מנפיקה", Description = "מדינה מנפיקה")]
    [Description("מדינה מנפיקה")]
    IssuingCountry = 28,

    [Display(Name = "יישוב הצהרת יצואן", Description = "יישוב הצהרת יצואן")]
    [Description("יישוב הצהרת יצואן")]
    CityOfDeclaration = 29,

    [Display(Name = "מדינת הצהרת יצואן", Description = "מדינת הצהרת יצואן")]
    [Description("מדינת הצהרת יצואן")]
    CountryOfDeclaration = 30,

    [Display(Name = "תאריך הצהרת יצואן", Description = "תאריך הצהרת יצואן")]
    [Description("תאריך הצהרת יצואן")]
    DateOfDeclaration = 31,

    [Display(Name = "המצהיר הוא היצרן", Description = "המצהיר הוא היצרן")]
    [Description("המצהיר הוא היצרן")]
    IsDeclaredByManufacturer = 32,

    [Display(Name = "המצהיר הוא היצואן", Description = "המצהיר הוא היצואן")]
    [Description("המצהיר הוא היצואן")]
    IsDeclaredByExporter = 33,

    [Display(Name = "תאריך היצוא", Description = "תאריך היצוא")]
    [Description("תאריך היצוא")]
    ExportDate = 34,

    [Display(Name = "מדינת היצוא", Description = "מדינת היצוא")]
    [Description("מדינת היצוא")]
    ExportCountry = 35,

    [Display(Name = "מספר שטר מטען יבוא לישראל", Description = "מספר שטר מטען יבוא לישראל")]
    [Description("מספר שטר מטען יבוא לישראל")]
    ImportBillOfLadingNum = 36,

    [Display(Name = "נמל המוצא/ יצוא", Description = "נמל המוצא/ יצוא")]
    [Description("נמל המוצא/ יצוא")]
    ExportPort = 37,

    [Display(Name = "תאריך היבוא לישראל", Description = "תאריך היבוא לישראל")]
    [Description("תאריך היבוא לישראל")]
    ImportDate = 38,

    [Display(Name = "מספר שטר מטען יצוא מישראל", Description = "מספר שטר מטען יצוא מישראל")]
    [Description("מספר שטר מטען יצוא מישראל")]
    ExportBillOFLadingNum = 39,

    [Display(Name = "מדינת הביניים", Description = "מדינת הביניים")]
    [Description("מדינת הביניים")]
    TransirCountry = 40,

    [Display(Name = "נמל כניסה לישראל", Description = "נמל כניסה לישראל")]
    [Description("נמל כניסה לישראל")]
    PortOfEntrance = 41,

    [Display(Name = "תאריך יציאה משוער מישראל", Description = "תאריך יציאה משוער מישראל")]
    [Description("תאריך יציאה משוער מישראל")]
    ExpectedExitDate = 42,

    [Display(Name = "נמל יציאה מישראל", Description = "נמל יציאה מישראל")]
    [Description("נמל יציאה מישראל")]
    ExitPort = 43,

    [Display(Name = "תיאור הטובין בתעודת מעבר", Description = "תיאור הטובין בתעודת מעבר")]
    [Description("תיאור הטובין בתעודת מעבר")]
    GoodsDescription = 44,

    [Display(Name = "שם החברה המצהירה", Description = "שם החברה המצהירה")]
    [Description("שם החברה המצהירה")]
    DeclaringCompany = 45,

    [Display(Name = "שם המצהיר (אדם ספציפי)", Description = "שם המצהיר (אדם ספציפי)")]
    [Description("שם המצהיר (אדם ספציפי)")]
    DeclaringPerson = 46,

    [Display(Name = "תפקיד המצהיר", Description = "תפקיד המצהיר")]
    [Description("תפקיד המצהיר")]
    DeclaringPosition = 47,

    [Display(Name = "מספר מצהר", Description = "מספר מצהר")]
    [Description("מספר מצהר")]
    ManifestNum = 48,
}
