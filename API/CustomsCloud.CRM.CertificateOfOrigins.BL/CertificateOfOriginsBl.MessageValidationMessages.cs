using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// Central message catalogue for the GetPC_MSG2280_2281 field-validation engine — the .NET 10 stand-in for the legacy
// indirection SystemTablesUtil.GetUIMessageWithEnglishAndLevel(EMessages.X) (one code → one text, resolved centrally),
// mirroring the existing EReconciliationMessage pattern (#34).
//
// Each enum member's NUMERIC VALUE is the real UIMessage id, so the exception carries the true EMessages code on
// ExceptionType. The Hebrew texts are the authoritative UIMessage.UserFriendlyMessage values (confirmed against the
// module's UIMessage export, 2026-08).
// TODO(migration): when the resx / BaseValidationMessages pipeline lands (see Program.cs), source the text from
// ValidationMessages keyed by ExceptionType and drop this local catalogue.
public partial class CertificateOfOriginsBl
{
    private enum EMessageCode
    {
        CustomerNotInCustomers = 4978,
        CertificateToCancelIncorrectStatus = 4984,
        ReplacementReasonMissing = 4986,
        ExportDeclarationMissing = 4987,
        SecondCountryNotInAgreement = 4990,
        IllegalExporterCountry = 4991,
        IllegalFirstCountryInAgreement = 4992,
        SecondCountryRequired = 4993,
        GroupOfCountriesNotInAgreement = 4994,
        ImporterCountryNotInAgreement = 4995,
        ImporterCountryRequired = 4996,
        OriginCountryNotInAgreement = 4997,
        OriginCountryRequired = 4998,
        OriginGroupOfCountriesNotInAgreement = 4999,
        DestinationCountryNotInAgreement = 5000,
        DestinationCountryRequired = 5001,
        DestinationGroupOfCountriesNotInAgreement = 5002,
        CumulationCountryRequired = 5003,
        CumulationCountryNotInAgreement = 5004,
        CumulationGroupOfCountriesNotInAgreement = 5005,
        PlaceOfManufactureAndZipcodeRequired = 5006,
        CustomsHouseNotInTable = 5007,
        IssuingCountryIllegal = 5008,
        ExporterDecCountryIllegal = 5009,
        ExporterDecDateIllegal = 5010,
        DeclaringExporter = 5011,
        ItemNumberNotFound = 5012,
        OriginCriteriaMissing = 5013,
        OriginCriteriaIllegal = 5014,
        ExportDateIllegal = 5015,
        ExportCounrtyIllegal = 5016,
        ImportDatetIllegal = 5017,
        TransirCountryIllegal = 5018,
        ManifestIdMissing = 5019,
        RequestReasonNotExist = 5020,
        CountryAndCountryGroup = 5040,
        ExitDateIllegal = 5095,
        AgentInUpdateDifferent = 11908,
        IllegalCertificateTypeUpdate = 11909,
        CertificateDoesntExist = 13680,
        MustSendCertificateID = 14026318,
        ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee = 14027036,
        ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked = 14027037,
        TheNumOfCharactersInTheZipcodeIsLessThan7 = 14027168,
        ACustomsItemMustContainAtLeast6Digits = 14027169,
        CustomsItemRequired = 14027172,
        ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration = 14027173,
        TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate = 14027174,
        TheDescriptionLengthCannotExceed255Characters = 14027343,
        CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration = 14027175,
        ExportCountryDoesNotExistInTheCountryTable = 14027393,
        CityOfDeclarationDoesNotExistInTheCitiesTable = 14027394,
        ValueNull = 6,
        TheValueInFieldNotExistsInSystem = 650,
        MandatoryValue = 2387,
        MandatoryNullValue = 2942,
        FieldMandatoryWhenTheAnotherField = 4832,
        RequiredContainerIsoCodeFieldIfItIsAContainer = 14026376,
    }

    // code → authoritative Hebrew text (UIMessage.UserFriendlyMessage). {0}/{1}/{2} fill the legacy message parameters.
    private static readonly IReadOnlyDictionary<EMessageCode, string> MessageTexts = new Dictionary<EMessageCode, string>
    {
        [EMessageCode.CustomerNotInCustomers] = "לקוח {0} לא קיים במאגר לקוחות",
        [EMessageCode.CertificateToCancelIncorrectStatus] = "בהחלפת תעודה סטטוס התעודה לביטול חייב להיות מאושרת לפרסום באינטרנט",
        [EMessageCode.ReplacementReasonMissing] = "סיבת בקשה להחלפת תעודה חסרה",
        [EMessageCode.ExportDeclarationMissing] = "מספר הצהרת יצוא חסר",
        [EMessageCode.SecondCountryNotInAgreement] = "מדינה שניה אינה שייכת להסכם הסחר",
        [EMessageCode.IllegalExporterCountry] = "מדינת היצואן חייבת להיות ישראל",
        [EMessageCode.IllegalFirstCountryInAgreement] = "מדינה ראשונה בהסכם חייבת להיות ישראל",
        [EMessageCode.SecondCountryRequired] = "חסרה מדינה שניה או קבוצת מדינות בהסכם",
        [EMessageCode.GroupOfCountriesNotInAgreement] = "קבוצת מדינות אינה שייכת להסכם הסחר",
        [EMessageCode.ImporterCountryNotInAgreement] = "מדינת יבואן/נשגר אינה שייכת להסכם הסחר",
        [EMessageCode.ImporterCountryRequired] = "חסרה מדינת יבואן/נשגר",
        [EMessageCode.OriginCountryNotInAgreement] = "מדינת המקור אינה שייכת להסכם הסחר",
        [EMessageCode.OriginCountryRequired] = "חסרה מדינת המקור או קבוצת מדינות המקור",
        [EMessageCode.OriginGroupOfCountriesNotInAgreement] = "קבוצת מדינות מקור אינה שייכת להסכם הסחר",
        [EMessageCode.DestinationCountryNotInAgreement] = "מדינת היעד אינה שייכת להסכם הסחר",
        [EMessageCode.DestinationCountryRequired] = "חסרה מדינת היעד או קבוצת מדינות היעד",
        [EMessageCode.DestinationGroupOfCountriesNotInAgreement] = "קבוצת מדינות היעד אינה שייכת להסכם הסחר",
        [EMessageCode.CumulationCountryRequired] = "חסרה מדינת הצבירה או קבוצת מדינות הצבירה",
        [EMessageCode.CumulationCountryNotInAgreement] = "מדינת הצבירה אינה שייכת להסכם הסחר",
        [EMessageCode.CumulationGroupOfCountriesNotInAgreement] = "קבוצת מדינות הצבירה אינה שייכת להסכם הסחר",
        [EMessageCode.PlaceOfManufactureAndZipcodeRequired] = "יש לציין מקום ייצור הטובין ומיקוד",
        [EMessageCode.CustomsHouseNotInTable] = "ערך בשדה בית מכס אינו קיים בטבלת בתי מכס",
        [EMessageCode.IssuingCountryIllegal] = "מדינה מנפיקה אינה יכולה להיות שונה מישראל",
        [EMessageCode.ExporterDecCountryIllegal] = "מדינת הצהרת יצואן אינה יכולה להיות שונה מישראל",
        [EMessageCode.ExporterDecDateIllegal] = "תאריך הצהרת יצואן צריך להיות שווה להיום או מוקדם ממנו, עד 5 ימים אחורה",
        [EMessageCode.DeclaringExporter] = "השדה IsDeclaredByExporter - `המצהיר הוא היצואן` חייב להיות TRUE",
        [EMessageCode.ItemNumberNotFound] = "פרט מכס לא קיים בספר המכס",
        [EMessageCode.OriginCriteriaMissing] = "קריטריון העדפה חסר",
        [EMessageCode.OriginCriteriaIllegal] = "ערך לא חוקי לקריטריון העדפה",
        [EMessageCode.ExportDateIllegal] = "תאריך היצוא יכול להיות היסטורי ועד שלושה חודשים קדימה",
        [EMessageCode.ExportCounrtyIllegal] = "מדינת יצוא אינה יכולה להיות ישראל",
        [EMessageCode.ImportDatetIllegal] = "תאריך היבוא לישראל צריך להיות שווה או מוקדם מתאריך היצוא, עד שנה אחורה",
        [EMessageCode.TransirCountryIllegal] = "מדינת הביניים אינה יכולה להיות שונה מישראל",
        [EMessageCode.ManifestIdMissing] = "לא הוזן מספר מצהר או מספר הצהרה עבור תעודה מסוג non manipulation",
        [EMessageCode.RequestReasonNotExist] = "סיבת בקשה לא קיימת במערכת",
        [EMessageCode.CountryAndCountryGroup] = "אין לספק מדינה וקבוצת מדינות לאותו עניין",
        [EMessageCode.ExitDateIllegal] = "תאריך היציאה המשוער מישראל צריך להיות שווה או גדול מן התאריך הנוכחי, עד שלושה חודשים קדימה",
        [EMessageCode.AgentInUpdateDifferent] = "סוכן מכס במסר שונה מן הסוכן בבקשה {0} להנפקת תעודת מקור",
        [EMessageCode.IllegalCertificateTypeUpdate] = "אין לתקן סוג תעודה {0} לסוג תעודה {1}",
        [EMessageCode.CertificateDoesntExist] = "תעודה מספר {0} אינה קיימת במערכת",
        [EMessageCode.MustSendCertificateID] = "חובה לספק מספר תעודה לפעולה המבוקשת",
        [EMessageCode.ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee] = "לא ניתן לשדר תיקון לתעודה {0} כאשר יש משימה לעובד מכס",
        [EMessageCode.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked] = "לא ניתן לשדר סוג בקשה {0} לתעודה בסטטוס {1}",
        [EMessageCode.TheNumOfCharactersInTheZipcodeIsLessThan7] = "מספר התוים במיקוד קטן מ-7",
        [EMessageCode.ACustomsItemMustContainAtLeast6Digits] = "פרט מכס חייב להכיל לפחות 6 ספרות",
        [EMessageCode.CustomsItemRequired] = "חסר שדה פרט מכס",
        [EMessageCode.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration] = "לא ניתן לשדר מספר הצהרה שלא הוגשה או הצהרה מבוטלת",
        [EMessageCode.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate] = "יש לבטל את ההצהרה המקושרת {0} קודם ביטול התעודה",
        [EMessageCode.TheDescriptionLengthCannotExceed255Characters] = "אורך התיאור לא יכול להיות גדול מ255 תווים",
        [EMessageCode.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration] = "לא ניתן לשדר תעודה כאשר קיים תהליך תיקון על ההצהרה {0} המקושרת לתעודה",
        [EMessageCode.ExportCountryDoesNotExistInTheCountryTable] = "מדינת היצוא אינה קיימת בטבלת המדינות",
        [EMessageCode.CityOfDeclarationDoesNotExistInTheCitiesTable] = "הערך בשדה יישוב הצהרת יצואן אינו קיים בטבלת היישובים.",

        [EMessageCode.MandatoryValue] = "אין ערך בשדה חובה {0}",
        [EMessageCode.MandatoryNullValue] = "הערך בשדה {0} חובה שיהיה ריק",
        [EMessageCode.TheValueInFieldNotExistsInSystem] = "הערך בשדה {0} לא קיים במערכת",
        [EMessageCode.FieldMandatoryWhenTheAnotherField] = "השדה {0} הינו שדה חובה כאשר הערך בשדה  {1} הינו {2}",
        [EMessageCode.ValueNull] = "יש להזין ערך בשדה {0}",
        [EMessageCode.RequiredContainerIsoCodeFieldIfItIsAContainer] = "שדה סוג מכולה חובה אם מדובר במכולה",
    };

    // Build the exception DTO for a message code, carrying the real EMessages code (ExceptionType) — the .NET 10
    // equivalent of new InfException(EMessages.X, parameters). Optional args fill the {0}/{1}/{2} placeholders.
    private static CertificateOfOriginExceptionDto BuildMessageException(EMessageCode code, params object?[] args)
    {
        var template = MessageTexts.TryGetValue(code, out var text) ? text : code.ToString();
        var description = args.Length > 0
            ? string.Format(System.Globalization.CultureInfo.InvariantCulture, template, args)
            : template;
        return new CertificateOfOriginExceptionDto
        {
            ExceptionLevel = (int)EExceptionLevel.Error,
            ExceptionDescription = description,
            ExceptionType = (int)code,
        };
    }
}
