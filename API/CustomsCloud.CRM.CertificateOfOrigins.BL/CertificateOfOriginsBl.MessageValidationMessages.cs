using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// Central message catalogue for the GetPC_MSG2280_2281 field-validation engine — the .NET 10 stand-in for the legacy
// indirection SystemTablesUtil.GetUIMessageWithEnglishAndLevel(EMessages.X) (one code → one text, resolved centrally),
// mirroring the existing EReconciliationMessage pattern (#34). Each enum member maps 1:1 to a legacy EMessages code
// (noted inline), so the exception carries the code (ExceptionType) instead of a scattered string literal.
//
// TODO(migration): the Hebrew texts here come from the legacy UIMessage table; the English text + the generic resx
// pipeline (ValidationMessages / BaseValidationMessages — see Program.cs) are still blocked repo-wide. When that lands,
// source the text from ValidationMessages keyed by ExceptionType and drop this local catalogue.
public partial class CertificateOfOriginsBl
{
    private enum EMessageCode
    {
        MandatoryValue,                         // EMessages.MandatoryValue
        MandatoryNullValue,                     // EMessages.MandatoryNullValue
        CustomerNotInCustomers,                 // EMessages.CustomerNotInCustomers
        ExportCountryDoesNotExistInTheCountryTable, // EMessages.ExportCountryDoesNotExistInTheCountryTable
        IllegalExporterCountry,                 // EMessages.IllegalExporterCountry
        IssuingCountryIllegal,                  // EMessages.IssuingCountryIllegal
        ExporterDecCountryIllegal,              // EMessages.ExporterDecCountryIllegal
        TransirCountryIllegal,                  // EMessages.TransirCountryIllegal
        ExportCounrtyIllegal,                   // EMessages.ExportCounrtyIllegal
        IllegalFirstCountryInAgreement,         // EMessages.IllegalFirstCountryInAgreement
        SecondCountryNotInAgreement,            // EMessages.SecondCountryNotInAgreement
        ImporterCountryNotInAgreement,          // EMessages.ImporterCountryNotInAgreement
        OriginCountryNotInAgreement,            // EMessages.OriginCountryNotInAgreement
        CumulationCountryNotInAgreement,        // EMessages.CumulationCountryNotInAgreement
        GroupOfCountriesNotInAgreement,         // EMessages.GroupOfCountriesNotInAgreement
        OriginGroupOfCountriesNotInAgreement,   // EMessages.OriginGroupOfCountriesNotInAgreement
        DestinationGroupOfCountriesNotInAgreement, // EMessages.DestinationGroupOfCountriesNotInAgreement
        CumulationGroupOfCountriesNotInAgreement, // EMessages.CumulationGroupOfCountriesNotInAgreement
        TheValueInFieldNotExistsInSystem,       // EMessages.TheValueInFieldNotExistsInSystem
        ExporterDecDateIllegal,                 // EMessages.ExporterDecDateIllegal
        ExportDateIllegal,                      // EMessages.ExportDateIllegal
        ExitDateIllegal,                        // EMessages.ExitDateIllegal
        DeclaringExporter,                      // EMessages.DeclaringExporter
        CityOfDeclarationDoesNotExistInTheCitiesTable, // EMessages.CityOfDeclarationDoesNotExistInTheCitiesTable
        CustomsHouseNotInTable,                 // EMessages.CustomsHouseNotInTable
        CertificateDoesntExist,                 // EMessages.CertificateDoesntExist
        MustSendCertificateID,                  // EMessages.MustSendCertificateID

        // Cross-field (CheckFields) codes.
        ImportDatetIllegal,                     // EMessages.ImportDatetIllegal
        ImporterCountryRequired,                // EMessages.ImporterCountryRequired
        CumulationCountryRequired,              // EMessages.CumulationCountryRequired
        CountryAndCountryGroup,                 // EMessages.CountryAndCountryGroup
        PlaceOfManufactureAndZipcodeRequired,   // EMessages.PlaceOfManufactureAndZipcodeRequired
        TheNumOfCharactersInTheZipcodeIsLessThan7, // EMessages.TheNumOfCharactersInTheZipcodeIsLessThan7
        FieldMandatoryWhenTheAnotherField,      // EMessages.FieldMandatoryWhenTheAnotherField
        SecondCountryRequired,                  // EMessages.SecondCountryRequired
        OriginCountryRequired,                  // EMessages.OriginCountryRequired
        DestinationCountryRequired,             // EMessages.DestinationCountryRequired
        DestinationCountryNotInAgreement,       // EMessages.DestinationCountryNotInAgreement
        ManifestIdMissing,                      // EMessages.ManifestIdMissing
        ExportDeclarationMissing,               // EMessages.ExportDeclarationMissing

        // Invoice / item (CheckAndConvertInvoiceDetails) codes.
        RequiredContainerIsoCodeFieldIfItIsAContainer, // EMessages.RequiredContainerISOCodeFieldIfItIsAContainer
        TheDescriptionLengthCannotExceed255Characters, // EMessages.TheDescriptionLengthCannotExceed255Characters
        ItemNumberNotFound,                     // EMessages.ItemNumberNotFound
        ACustomsItemMustContainAtLeast6Digits,  // EMessages.ACustomsItemMustContainAtLeast6Digits
        CustomsItemRequired,                    // EMessages.CustomsItemRequired
        OriginCriteriaMissing,                  // EMessages.OriginCriteriaMissing
        OriginCriteriaIllegal,                  // EMessages.OriginCriteriaIllegal
    }

    // code → Hebrew text (legacy UIMessage). {0} is the field value / id where the legacy message carried a parameter.
    private static readonly IReadOnlyDictionary<EMessageCode, string> MessageTexts = new Dictionary<EMessageCode, string>
    {
        [EMessageCode.MandatoryValue] = "שדה חובה חסר: {0}",
        [EMessageCode.MandatoryNullValue] = "ערך חובה חסר: {0}",
        [EMessageCode.CustomerNotInCustomers] = "היצואן {0} אינו קיים במרשם הלקוחות",
        [EMessageCode.ExportCountryDoesNotExistInTheCountryTable] = "הארץ {0} אינה קיימת בטבלת הארצות",
        [EMessageCode.IllegalExporterCountry] = "ארץ יצואן אינה חוקית",
        [EMessageCode.IssuingCountryIllegal] = "ארץ הנפקה אינה חוקית",
        [EMessageCode.ExporterDecCountryIllegal] = "ארץ הצהרת יצואן אינה חוקית",
        [EMessageCode.TransirCountryIllegal] = "ארץ מעבר אינה חוקית",
        [EMessageCode.ExportCounrtyIllegal] = "ארץ ייצוא אינה חוקית",
        [EMessageCode.IllegalFirstCountryInAgreement] = "הארץ הראשונה בהסכם אינה חוקית",
        [EMessageCode.SecondCountryNotInAgreement] = "הארץ השנייה אינה בהסכם",
        [EMessageCode.ImporterCountryNotInAgreement] = "ארץ הנשגר אינה בהסכם",
        [EMessageCode.OriginCountryNotInAgreement] = "ארץ המקור אינה בהסכם",
        [EMessageCode.CumulationCountryNotInAgreement] = "ארץ המצטבר אינה בהסכם",
        [EMessageCode.GroupOfCountriesNotInAgreement] = "קבוצת הארצות אינה בהסכם",
        [EMessageCode.OriginGroupOfCountriesNotInAgreement] = "קבוצת ארצות המקור אינה בהסכם",
        [EMessageCode.DestinationGroupOfCountriesNotInAgreement] = "קבוצת ארצות היעד אינה בהסכם",
        [EMessageCode.CumulationGroupOfCountriesNotInAgreement] = "קבוצת ארצות המצטבר אינה בהסכם",
        [EMessageCode.TheValueInFieldNotExistsInSystem] = "הערך בשדה {0} אינו קיים במערכת",
        [EMessageCode.ExporterDecDateIllegal] = "תאריך הצהרת יצואן אינו חוקי",
        [EMessageCode.ExportDateIllegal] = "תאריך ייצוא אינו חוקי",
        [EMessageCode.ExitDateIllegal] = "תאריך יציאה צפוי אינו חוקי",
        [EMessageCode.DeclaringExporter] = "ההצהרה חייבת להיות על ידי היצואן",
        [EMessageCode.CityOfDeclarationDoesNotExistInTheCitiesTable] = "עיר ההצהרה {0} אינה קיימת בטבלת הערים",
        [EMessageCode.CustomsHouseNotInTable] = "בית המכס אינו קיים בטבלה",
        [EMessageCode.CertificateDoesntExist] = "התעודה {0} אינה קיימת",
        [EMessageCode.MustSendCertificateID] = "יש לשלוח מזהה תעודה",
        [EMessageCode.ImportDatetIllegal] = "תאריך היבוא אינו חוקי",
        [EMessageCode.ImporterCountryRequired] = "ארץ הנשגר הינה שדה חובה",
        [EMessageCode.CumulationCountryRequired] = "ארץ המצטבר הינה שדה חובה",
        [EMessageCode.CountryAndCountryGroup] = "לא ניתן להזין גם ארץ וגם קבוצת ארצות",
        [EMessageCode.PlaceOfManufactureAndZipcodeRequired] = "מקום הייצור והמיקוד הינם שדות חובה",
        [EMessageCode.TheNumOfCharactersInTheZipcodeIsLessThan7] = "מספר התווים במיקוד קטן מ-7",
        [EMessageCode.FieldMandatoryWhenTheAnotherField] = "השדה {0} הינו שדה חובה כאשר השדה {1} הוא {2}",
        [EMessageCode.SecondCountryRequired] = "הארץ השנייה הינה שדה חובה",
        [EMessageCode.OriginCountryRequired] = "ארץ המקור הינה שדה חובה",
        [EMessageCode.DestinationCountryRequired] = "ארץ היעד הינה שדה חובה",
        [EMessageCode.DestinationCountryNotInAgreement] = "ארץ היעד אינה בהסכם",
        [EMessageCode.ManifestIdMissing] = "מספר מניפסט חסר",
        [EMessageCode.ExportDeclarationMissing] = "מספר רשימון יצוא חסר",
        [EMessageCode.RequiredContainerIsoCodeFieldIfItIsAContainer] = "יש להזין קוד ISO של מכולה כאשר סוג האריזה הוא מכולה",
        [EMessageCode.TheDescriptionLengthCannotExceed255Characters] = "אורך התיאור אינו יכול לעלות על 255 תווים",
        [EMessageCode.ItemNumberNotFound] = "מספר הפריט לא נמצא",
        [EMessageCode.ACustomsItemMustContainAtLeast6Digits] = "פריט מכס חייב להכיל לפחות 6 ספרות",
        [EMessageCode.CustomsItemRequired] = "פריט מכס הינו שדה חובה",
        [EMessageCode.OriginCriteriaMissing] = "קריטריון מקור חסר",
        [EMessageCode.OriginCriteriaIllegal] = "קריטריון מקור אינו חוקי",
    };

    // Build the exception DTO for a message code, carrying the code (ExceptionType) — the .NET 10 equivalent of
    // new InfException(EMessages.X, parameters). Optional args fill the {0} placeholder of the text.
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

            // TODO(migration): the real numeric EMessages code (legacy GetUIMessageWithEnglishAndLevel). Kept as the
            // local catalogue ordinal until the SystemTables message table / ValidationMessages mapping lands.
            ExceptionType = (int)code,
        };
    }
}
