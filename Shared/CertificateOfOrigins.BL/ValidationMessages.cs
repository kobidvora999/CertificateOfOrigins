using CustomsCloud.InfrastructureCore.Interfaces.WebApi;

namespace CertificateOfOrigins.BL;

// The module's validation and business messages (legacy EMessages), exposed to clients through .AddValidationMessages
// in Program.cs. The texts live in ErrorMessagesResources.resx: the exact UIMessage texts from the legacy DB, as generated
// into MalamTeam.Infrastructure...Enums\EMessages1.cs (each entry's comment carries its legacy id). Levels are each row's
// UIMessageTypeID (Shared.General_enum_UIMessage); the reconciliation decides its outcome by the InfException level instead.
// Text-only entries (reasons, remarks, task info) stay in the resx only.
public class ValidationMessages : BaseValidationMessages<ValidationMessages>
{
    public static ValidationMessage GeneralException { get; } =
        new ValidationMessage(nameof(GeneralException), ErrorMessagesResources.GeneralException, ValidationMessageLevel.Error);

    public static ValidationMessage ConcurrencyErrorTryAgain { get; } =
        new ValidationMessage(nameof(ConcurrencyErrorTryAgain), ErrorMessagesResources.ConcurrencyErrorTryAgain, ValidationMessageLevel.Error);

    public static ValidationMessage CustomerNotInCustomers { get; } =
        new ValidationMessage(nameof(CustomerNotInCustomers), ErrorMessagesResources.CustomerNotInCustomers, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateToCancelIncorrectStatus { get; } =
        new ValidationMessage(nameof(CertificateToCancelIncorrectStatus), ErrorMessagesResources.CertificateToCancelIncorrectStatus, ValidationMessageLevel.Error);

    public static ValidationMessage ReplacementReasonMissing { get; } =
        new ValidationMessage(nameof(ReplacementReasonMissing), ErrorMessagesResources.ReplacementReasonMissing, ValidationMessageLevel.Error);

    public static ValidationMessage ExportDeclarationMissing { get; } =
        new ValidationMessage(nameof(ExportDeclarationMissing), ErrorMessagesResources.ExportDeclarationMissing, ValidationMessageLevel.Error);

    public static ValidationMessage SecondCountryNotInAgreement { get; } =
        new ValidationMessage(nameof(SecondCountryNotInAgreement), ErrorMessagesResources.SecondCountryNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage IllegalExporterCountry { get; } =
        new ValidationMessage(nameof(IllegalExporterCountry), ErrorMessagesResources.IllegalExporterCountry, ValidationMessageLevel.Error);

    public static ValidationMessage IllegalFirstCountryInAgreement { get; } =
        new ValidationMessage(nameof(IllegalFirstCountryInAgreement), ErrorMessagesResources.IllegalFirstCountryInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage SecondCountryRequired { get; } =
        new ValidationMessage(nameof(SecondCountryRequired), ErrorMessagesResources.SecondCountryRequired, ValidationMessageLevel.Error);

    public static ValidationMessage GroupOfCountriesNotInAgreement { get; } =
        new ValidationMessage(nameof(GroupOfCountriesNotInAgreement), ErrorMessagesResources.GroupOfCountriesNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage ImporterCountryNotInAgreement { get; } =
        new ValidationMessage(nameof(ImporterCountryNotInAgreement), ErrorMessagesResources.ImporterCountryNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage ImporterCountryRequired { get; } =
        new ValidationMessage(nameof(ImporterCountryRequired), ErrorMessagesResources.ImporterCountryRequired, ValidationMessageLevel.Error);

    public static ValidationMessage OriginCountryNotInAgreement { get; } =
        new ValidationMessage(nameof(OriginCountryNotInAgreement), ErrorMessagesResources.OriginCountryNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage OriginCountryRequired { get; } =
        new ValidationMessage(nameof(OriginCountryRequired), ErrorMessagesResources.OriginCountryRequired, ValidationMessageLevel.Error);

    public static ValidationMessage OriginGroupOfCountriesNotInAgreement { get; } =
        new ValidationMessage(nameof(OriginGroupOfCountriesNotInAgreement), ErrorMessagesResources.OriginGroupOfCountriesNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationCountryNotInAgreement { get; } =
        new ValidationMessage(nameof(DestinationCountryNotInAgreement), ErrorMessagesResources.DestinationCountryNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationCountryRequired { get; } =
        new ValidationMessage(nameof(DestinationCountryRequired), ErrorMessagesResources.DestinationCountryRequired, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationGroupOfCountriesNotInAgreement { get; } =
        new ValidationMessage(nameof(DestinationGroupOfCountriesNotInAgreement), ErrorMessagesResources.DestinationGroupOfCountriesNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage CumulationCountryRequired { get; } =
        new ValidationMessage(nameof(CumulationCountryRequired), ErrorMessagesResources.CumulationCountryRequired, ValidationMessageLevel.Error);

    public static ValidationMessage CumulationCountryNotInAgreement { get; } =
        new ValidationMessage(nameof(CumulationCountryNotInAgreement), ErrorMessagesResources.CumulationCountryNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage CumulationGroupOfCountriesNotInAgreement { get; } =
        new ValidationMessage(nameof(CumulationGroupOfCountriesNotInAgreement), ErrorMessagesResources.CumulationGroupOfCountriesNotInAgreement, ValidationMessageLevel.Error);

    public static ValidationMessage PlaceOfManufactureAndZipcodeRequired { get; } =
        new ValidationMessage(nameof(PlaceOfManufactureAndZipcodeRequired), ErrorMessagesResources.PlaceOfManufactureAndZipcodeRequired, ValidationMessageLevel.Error);

    public static ValidationMessage CustomsHouseNotInTable { get; } =
        new ValidationMessage(nameof(CustomsHouseNotInTable), ErrorMessagesResources.CustomsHouseNotInTable, ValidationMessageLevel.Error);

    public static ValidationMessage IssuingCountryIllegal { get; } =
        new ValidationMessage(nameof(IssuingCountryIllegal), ErrorMessagesResources.IssuingCountryIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ExporterDecCountryIllegal { get; } =
        new ValidationMessage(nameof(ExporterDecCountryIllegal), ErrorMessagesResources.ExporterDecCountryIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ExporterDecDateIllegal { get; } =
        new ValidationMessage(nameof(ExporterDecDateIllegal), ErrorMessagesResources.ExporterDecDateIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage DeclaringExporter { get; } =
        new ValidationMessage(nameof(DeclaringExporter), ErrorMessagesResources.DeclaringExporter, ValidationMessageLevel.Error);

    public static ValidationMessage ItemNumberNotFound { get; } =
        new ValidationMessage(nameof(ItemNumberNotFound), ErrorMessagesResources.ItemNumberNotFound, ValidationMessageLevel.Error);

    public static ValidationMessage OriginCriteriaMissing { get; } =
        new ValidationMessage(nameof(OriginCriteriaMissing), ErrorMessagesResources.OriginCriteriaMissing, ValidationMessageLevel.Error);

    public static ValidationMessage OriginCriteriaIllegal { get; } =
        new ValidationMessage(nameof(OriginCriteriaIllegal), ErrorMessagesResources.OriginCriteriaIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ExportDateIllegal { get; } =
        new ValidationMessage(nameof(ExportDateIllegal), ErrorMessagesResources.ExportDateIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ExportCounrtyIllegal { get; } =
        new ValidationMessage(nameof(ExportCounrtyIllegal), ErrorMessagesResources.ExportCounrtyIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ImportDatetIllegal { get; } =
        new ValidationMessage(nameof(ImportDatetIllegal), ErrorMessagesResources.ImportDatetIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage TransirCountryIllegal { get; } =
        new ValidationMessage(nameof(TransirCountryIllegal), ErrorMessagesResources.TransirCountryIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage ManifestIdMissing { get; } =
        new ValidationMessage(nameof(ManifestIdMissing), ErrorMessagesResources.ManifestIdMissing, ValidationMessageLevel.Error);

    public static ValidationMessage RequestReasonNotExist { get; } =
        new ValidationMessage(nameof(RequestReasonNotExist), ErrorMessagesResources.RequestReasonNotExist, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateTypeNotExist { get; } =
        new ValidationMessage(nameof(CertificateTypeNotExist), ErrorMessagesResources.CertificateTypeNotExist, ValidationMessageLevel.Error);

    public static ValidationMessage CountryAndCountryGroup { get; } =
        new ValidationMessage(nameof(CountryAndCountryGroup), ErrorMessagesResources.CountryAndCountryGroup, ValidationMessageLevel.Error);

    public static ValidationMessage ExitDateIllegal { get; } =
        new ValidationMessage(nameof(ExitDateIllegal), ErrorMessagesResources.ExitDateIllegal, ValidationMessageLevel.Error);

    public static ValidationMessage AgentInUpdateDifferent { get; } =
        new ValidationMessage(nameof(AgentInUpdateDifferent), ErrorMessagesResources.AgentInUpdateDifferent, ValidationMessageLevel.Error);

    public static ValidationMessage IllegalCertificateTypeUpdate { get; } =
        new ValidationMessage(nameof(IllegalCertificateTypeUpdate), ErrorMessagesResources.IllegalCertificateTypeUpdate, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateDoesntExist { get; } =
        new ValidationMessage(nameof(CertificateDoesntExist), ErrorMessagesResources.CertificateDoesntExist, ValidationMessageLevel.Error);

    public static ValidationMessage MustSendCertificateID { get; } =
        new ValidationMessage(nameof(MustSendCertificateID), ErrorMessagesResources.MustSendCertificateID, ValidationMessageLevel.Error);

    public static ValidationMessage ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee { get; } =
        new ValidationMessage(nameof(ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee), ErrorMessagesResources.ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee, ValidationMessageLevel.Error);

    public static ValidationMessage ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked { get; } =
        new ValidationMessage(nameof(ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked), ErrorMessagesResources.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked, ValidationMessageLevel.Error);

    public static ValidationMessage TheNumOfCharactersInTheZipcodeIsLessThan7 { get; } =
        new ValidationMessage(nameof(TheNumOfCharactersInTheZipcodeIsLessThan7), ErrorMessagesResources.TheNumOfCharactersInTheZipcodeIsLessThan7, ValidationMessageLevel.Error);

    public static ValidationMessage ACustomsItemMustContainAtLeast6Digits { get; } =
        new ValidationMessage(nameof(ACustomsItemMustContainAtLeast6Digits), ErrorMessagesResources.ACustomsItemMustContainAtLeast6Digits, ValidationMessageLevel.Error);

    public static ValidationMessage CustomsItemRequired { get; } =
        new ValidationMessage(nameof(CustomsItemRequired), ErrorMessagesResources.CustomsItemRequired, ValidationMessageLevel.Error);

    public static ValidationMessage ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration { get; } =
        new ValidationMessage(nameof(ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration), ErrorMessagesResources.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate { get; } =
        new ValidationMessage(nameof(TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate), ErrorMessagesResources.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate, ValidationMessageLevel.Error);

    public static ValidationMessage TheDescriptionLengthCannotExceed255Characters { get; } =
        new ValidationMessage(nameof(TheDescriptionLengthCannotExceed255Characters), ErrorMessagesResources.TheDescriptionLengthCannotExceed255Characters, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration { get; } =
        new ValidationMessage(nameof(CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration), ErrorMessagesResources.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage ExportCountryDoesNotExistInTheCountryTable { get; } =
        new ValidationMessage(nameof(ExportCountryDoesNotExistInTheCountryTable), ErrorMessagesResources.ExportCountryDoesNotExistInTheCountryTable, ValidationMessageLevel.Error);

    public static ValidationMessage CityOfDeclarationDoesNotExistInTheCitiesTable { get; } =
        new ValidationMessage(nameof(CityOfDeclarationDoesNotExistInTheCitiesTable), ErrorMessagesResources.CityOfDeclarationDoesNotExistInTheCitiesTable, ValidationMessageLevel.Error);

    public static ValidationMessage ValueNull { get; } =
        new ValidationMessage(nameof(ValueNull), ErrorMessagesResources.ValueNull, ValidationMessageLevel.Error);

    public static ValidationMessage TheValueInFieldNotExistsInSystem { get; } =
        new ValidationMessage(nameof(TheValueInFieldNotExistsInSystem), ErrorMessagesResources.TheValueInFieldNotExistsInSystem, ValidationMessageLevel.Error);

    public static ValidationMessage MandatoryValue { get; } =
        new ValidationMessage(nameof(MandatoryValue), ErrorMessagesResources.MandatoryValue, ValidationMessageLevel.Error);

    public static ValidationMessage MandatoryNullValue { get; } =
        new ValidationMessage(nameof(MandatoryNullValue), ErrorMessagesResources.MandatoryNullValue, ValidationMessageLevel.Error);

    public static ValidationMessage FieldMandatoryWhenTheAnotherField { get; } =
        new ValidationMessage(nameof(FieldMandatoryWhenTheAnotherField), ErrorMessagesResources.FieldMandatoryWhenTheAnotherField, ValidationMessageLevel.Error);

    public static ValidationMessage RequiredContainerIsoCodeFieldIfItIsAContainer { get; } =
        new ValidationMessage(nameof(RequiredContainerIsoCodeFieldIfItIsAContainer), ErrorMessagesResources.RequiredContainerIsoCodeFieldIfItIsAContainer, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationCountryMismatch { get; } =
        new ValidationMessage(nameof(DestinationCountryMismatch), ErrorMessagesResources.DestinationCountryMismatch, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationGroupDiscrepancy { get; } =
        new ValidationMessage(nameof(DestinationGroupDiscrepancy), ErrorMessagesResources.DestinationGroupDiscrepancy, ValidationMessageLevel.Warning);

    public static ValidationMessage ExportDeclarationNotInSystem { get; } =
        new ValidationMessage(nameof(ExportDeclarationNotInSystem), ErrorMessagesResources.ExportDeclarationNotInSystem, ValidationMessageLevel.Error);

    public static ValidationMessage ExporterMismatch { get; } =
        new ValidationMessage(nameof(ExporterMismatch), ErrorMessagesResources.ExporterMismatch, ValidationMessageLevel.Error);

    public static ValidationMessage ImportReplacementNoTradeAgreement { get; } =
        new ValidationMessage(nameof(ImportReplacementNoTradeAgreement), ErrorMessagesResources.ImportReplacementNoTradeAgreement, ValidationMessageLevel.Warning);

    public static ValidationMessage OriginCountryMismatch { get; } =
        new ValidationMessage(nameof(OriginCountryMismatch), ErrorMessagesResources.OriginCountryMismatch, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateNumberNotInDealFile { get; } =
        new ValidationMessage(nameof(CertificateNumberNotInDealFile), ErrorMessagesResources.CertificateNumberNotInDealFile, ValidationMessageLevel.Error);

    public static ValidationMessage CustomsItemMismatch { get; } =
        new ValidationMessage(nameof(CustomsItemMismatch), ErrorMessagesResources.CustomsItemMismatch, ValidationMessageLevel.Error);

    public static ValidationMessage ExportInvoiceNotMatch { get; } =
        new ValidationMessage(nameof(ExportInvoiceNotMatch), ErrorMessagesResources.ExportInvoiceNotMatch, ValidationMessageLevel.Error);

    public static ValidationMessage CustomsItemInDeclarationNotInCertificate { get; } =
        new ValidationMessage(nameof(CustomsItemInDeclarationNotInCertificate), ErrorMessagesResources.CustomsItemInDeclarationNotInCertificate, ValidationMessageLevel.Warning);

    public static ValidationMessage FileExistForRequest { get; } =
        new ValidationMessage(nameof(FileExistForRequest), ErrorMessagesResources.FileExistForRequest, ValidationMessageLevel.Error);

    public static ValidationMessage InvalidIdentificationNumber { get; } =
        new ValidationMessage(nameof(InvalidIdentificationNumber), ErrorMessagesResources.InvalidIdentificationNumber, ValidationMessageLevel.Error);

    public static ValidationMessage NoCustomHouseForThisCountry { get; } =
        new ValidationMessage(nameof(NoCustomHouseForThisCountry), ErrorMessagesResources.NoCustomHouseForThisCountry, ValidationMessageLevel.Error);
}
