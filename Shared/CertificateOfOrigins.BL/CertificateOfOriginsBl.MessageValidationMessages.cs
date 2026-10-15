using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL;

// Central message catalogue for the GetPC_MSG2280_2281 field-validation engine — the .NET 10 stand-in for the legacy
// indirection SystemTablesUtil.GetUIMessageWithEnglishAndLevel(EMessages.X) (one code → one text, resolved centrally),
// mirroring the existing EReconciliationMessage pattern (#34).
//
// Each enum member's NUMERIC VALUE is the real UIMessage id, so the exception carries the true EMessages code on
// ExceptionType. The texts come from ErrorMessagesResources.resx (the UIMessage.UserFriendlyMessage values, confirmed
// against the module's UIMessage export, 2026-08; code 5022 against the legacy EMessages1.cs, 2026-10-15), and the same
// entries are published to clients through ValidationMessages. ValidationMessage carries no numeric code, so this enum
// keeps the code → message map.
public partial class CertificateOfOriginsBl
{
    private enum EMessageCode
    {
        GeneralException = 0,
        ConcurrencyErrorTryAgain = 1402474,
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
        CertificateTypeNotExist = 5022,
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
        [EMessageCode.GeneralException] = ErrorMessagesResources.GeneralException,
        [EMessageCode.ConcurrencyErrorTryAgain] = ErrorMessagesResources.ConcurrencyErrorTryAgain,
        [EMessageCode.CustomerNotInCustomers] = ErrorMessagesResources.CustomerNotInCustomers,
        [EMessageCode.CertificateToCancelIncorrectStatus] = ErrorMessagesResources.CertificateToCancelIncorrectStatus,
        [EMessageCode.ReplacementReasonMissing] = ErrorMessagesResources.ReplacementReasonMissing,
        [EMessageCode.ExportDeclarationMissing] = ErrorMessagesResources.ExportDeclarationMissing,
        [EMessageCode.SecondCountryNotInAgreement] = ErrorMessagesResources.SecondCountryNotInAgreement,
        [EMessageCode.IllegalExporterCountry] = ErrorMessagesResources.IllegalExporterCountry,
        [EMessageCode.IllegalFirstCountryInAgreement] = ErrorMessagesResources.IllegalFirstCountryInAgreement,
        [EMessageCode.SecondCountryRequired] = ErrorMessagesResources.SecondCountryRequired,
        [EMessageCode.GroupOfCountriesNotInAgreement] = ErrorMessagesResources.GroupOfCountriesNotInAgreement,
        [EMessageCode.ImporterCountryNotInAgreement] = ErrorMessagesResources.ImporterCountryNotInAgreement,
        [EMessageCode.ImporterCountryRequired] = ErrorMessagesResources.ImporterCountryRequired,
        [EMessageCode.OriginCountryNotInAgreement] = ErrorMessagesResources.OriginCountryNotInAgreement,
        [EMessageCode.OriginCountryRequired] = ErrorMessagesResources.OriginCountryRequired,
        [EMessageCode.OriginGroupOfCountriesNotInAgreement] = ErrorMessagesResources.OriginGroupOfCountriesNotInAgreement,
        [EMessageCode.DestinationCountryNotInAgreement] = ErrorMessagesResources.DestinationCountryNotInAgreement,
        [EMessageCode.DestinationCountryRequired] = ErrorMessagesResources.DestinationCountryRequired,
        [EMessageCode.DestinationGroupOfCountriesNotInAgreement] = ErrorMessagesResources.DestinationGroupOfCountriesNotInAgreement,
        [EMessageCode.CumulationCountryRequired] = ErrorMessagesResources.CumulationCountryRequired,
        [EMessageCode.CumulationCountryNotInAgreement] = ErrorMessagesResources.CumulationCountryNotInAgreement,
        [EMessageCode.CumulationGroupOfCountriesNotInAgreement] = ErrorMessagesResources.CumulationGroupOfCountriesNotInAgreement,
        [EMessageCode.PlaceOfManufactureAndZipcodeRequired] = ErrorMessagesResources.PlaceOfManufactureAndZipcodeRequired,
        [EMessageCode.CustomsHouseNotInTable] = ErrorMessagesResources.CustomsHouseNotInTable,
        [EMessageCode.IssuingCountryIllegal] = ErrorMessagesResources.IssuingCountryIllegal,
        [EMessageCode.ExporterDecCountryIllegal] = ErrorMessagesResources.ExporterDecCountryIllegal,
        [EMessageCode.ExporterDecDateIllegal] = ErrorMessagesResources.ExporterDecDateIllegal,
        [EMessageCode.DeclaringExporter] = ErrorMessagesResources.DeclaringExporter,
        [EMessageCode.ItemNumberNotFound] = ErrorMessagesResources.ItemNumberNotFound,
        [EMessageCode.OriginCriteriaMissing] = ErrorMessagesResources.OriginCriteriaMissing,
        [EMessageCode.OriginCriteriaIllegal] = ErrorMessagesResources.OriginCriteriaIllegal,
        [EMessageCode.ExportDateIllegal] = ErrorMessagesResources.ExportDateIllegal,
        [EMessageCode.ExportCounrtyIllegal] = ErrorMessagesResources.ExportCounrtyIllegal,
        [EMessageCode.ImportDatetIllegal] = ErrorMessagesResources.ImportDatetIllegal,
        [EMessageCode.TransirCountryIllegal] = ErrorMessagesResources.TransirCountryIllegal,
        [EMessageCode.ManifestIdMissing] = ErrorMessagesResources.ManifestIdMissing,
        [EMessageCode.RequestReasonNotExist] = ErrorMessagesResources.RequestReasonNotExist,
        [EMessageCode.CertificateTypeNotExist] = ErrorMessagesResources.CertificateTypeNotExist,
        [EMessageCode.CountryAndCountryGroup] = ErrorMessagesResources.CountryAndCountryGroup,
        [EMessageCode.ExitDateIllegal] = ErrorMessagesResources.ExitDateIllegal,
        [EMessageCode.AgentInUpdateDifferent] = ErrorMessagesResources.AgentInUpdateDifferent,
        [EMessageCode.IllegalCertificateTypeUpdate] = ErrorMessagesResources.IllegalCertificateTypeUpdate,
        [EMessageCode.CertificateDoesntExist] = ErrorMessagesResources.CertificateDoesntExist,
        [EMessageCode.MustSendCertificateID] = ErrorMessagesResources.MustSendCertificateID,
        [EMessageCode.ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee] = ErrorMessagesResources.ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee,
        [EMessageCode.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked] = ErrorMessagesResources.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked,
        [EMessageCode.TheNumOfCharactersInTheZipcodeIsLessThan7] = ErrorMessagesResources.TheNumOfCharactersInTheZipcodeIsLessThan7,
        [EMessageCode.ACustomsItemMustContainAtLeast6Digits] = ErrorMessagesResources.ACustomsItemMustContainAtLeast6Digits,
        [EMessageCode.CustomsItemRequired] = ErrorMessagesResources.CustomsItemRequired,
        [EMessageCode.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration] = ErrorMessagesResources.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration,
        [EMessageCode.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate] = ErrorMessagesResources.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate,
        [EMessageCode.TheDescriptionLengthCannotExceed255Characters] = ErrorMessagesResources.TheDescriptionLengthCannotExceed255Characters,
        [EMessageCode.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration] = ErrorMessagesResources.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration,
        [EMessageCode.ExportCountryDoesNotExistInTheCountryTable] = ErrorMessagesResources.ExportCountryDoesNotExistInTheCountryTable,
        [EMessageCode.CityOfDeclarationDoesNotExistInTheCitiesTable] = ErrorMessagesResources.CityOfDeclarationDoesNotExistInTheCitiesTable,

        [EMessageCode.MandatoryValue] = ErrorMessagesResources.MandatoryValue,
        [EMessageCode.MandatoryNullValue] = ErrorMessagesResources.MandatoryNullValue,
        [EMessageCode.TheValueInFieldNotExistsInSystem] = ErrorMessagesResources.TheValueInFieldNotExistsInSystem,
        [EMessageCode.FieldMandatoryWhenTheAnotherField] = ErrorMessagesResources.FieldMandatoryWhenTheAnotherField,
        [EMessageCode.ValueNull] = ErrorMessagesResources.ValueNull,
        [EMessageCode.RequiredContainerIsoCodeFieldIfItIsAContainer] = ErrorMessagesResources.RequiredContainerIsoCodeFieldIfItIsAContainer,
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
