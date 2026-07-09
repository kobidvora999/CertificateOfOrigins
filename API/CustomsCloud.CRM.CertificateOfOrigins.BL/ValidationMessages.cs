// TODO(blocking): BaseValidationMessages / ValidationMessage / ValidationMessageLevel are NOT present in
// CustomsCloud.InfrastructureCore.BL 1.10.26 — the latest version on the external NuGet feed (verified
// 2026-07-07). The infra exists internally; when the updated package ships:
//   1. update the CustomsCloud.InfrastructureCore.BL PackageReference,
//   2. remove the #if guard below,
//   3. re-enable .AddValidationMessages<ValidationMessages>() in Program.cs.
// Until then the flow uses ErrorMessagesResources directly (texts + levels are preserved in the BL logic).
#if VALIDATION_MESSAGES_INFRA
using CustomsCloud.InfrastructureCore.BL.Exceptions; // TODO(confirm): exact namespace of BaseValidationMessages

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// Validation messages of the certificate-vs-export-declaration match flow (UpdateCetrificateOfOrigins).
// Levels mirror the legacy InfException levels: explicit Warning where the legacy passed
// EExceptionLevel.Warning, Error otherwise (legacy treated a null level as Error).
internal class ValidationMessages : BaseValidationMessages<ValidationMessages>
{
    public static ValidationMessage OriginCountryIsNotMatchToOriginCountryInExportDeclaration { get; } =
        new(nameof(OriginCountryIsNotMatchToOriginCountryInExportDeclaration), ErrorMessagesResources.OriginCountryIsNotMatchToOriginCountryInExportDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage DestinationCountryIsNotMatchToDestinationCountryInExportDeclaration { get; } =
        new(nameof(DestinationCountryIsNotMatchToDestinationCountryInExportDeclaration), ErrorMessagesResources.DestinationCountryIsNotMatchToDestinationCountryInExportDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage CertificateNumberIsNotMatchToCertificateNumberInExportDealFile { get; } =
        new(nameof(CertificateNumberIsNotMatchToCertificateNumberInExportDealFile), ErrorMessagesResources.CertificateNumberIsNotMatchToCertificateNumberInExportDealFile, ValidationMessageLevel.Error);

    public static ValidationMessage ExporterNumberIsNotMatchToExporterNumberInExportDeclaration { get; } =
        new(nameof(ExporterNumberIsNotMatchToExporterNumberInExportDeclaration), ErrorMessagesResources.ExporterNumberIsNotMatchToExporterNumberInExportDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage ExportInvoiceIsNotMatchToExportInvoiceInExportDeclaration { get; } =
        new(nameof(ExportInvoiceIsNotMatchToExportInvoiceInExportDeclaration), ErrorMessagesResources.ExportInvoiceIsNotMatchToExportInvoiceInExportDeclaration, ValidationMessageLevel.Warning);

    public static ValidationMessage CustomsItemIsNotMatchToCustomsItemInExportDeclaration { get; } =
        new(nameof(CustomsItemIsNotMatchToCustomsItemInExportDeclaration), ErrorMessagesResources.CustomsItemIsNotMatchToCustomsItemInExportDeclaration, ValidationMessageLevel.Error);

    public static ValidationMessage ExportDeclarationNotInSystemForWarningMessage { get; } =
        new(nameof(ExportDeclarationNotInSystemForWarningMessage), ErrorMessagesResources.ExportDeclarationNotInSystemForWarningMessage, ValidationMessageLevel.Error);

    public static ValidationMessage DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer { get; } =
        new(nameof(DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer), ErrorMessagesResources.DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer, ValidationMessageLevel.Error);

    public static ValidationMessage ThereIsNoMerchandiseLinkedToImportDeclarationInTradeAgreement { get; } =
        new(nameof(ThereIsNoMerchandiseLinkedToImportDeclarationInTradeAgreement), ErrorMessagesResources.ThereIsNoMerchandiseLinkedToImportDeclarationInTradeAgreement, ValidationMessageLevel.Warning);

    public static ValidationMessage CustomsItemInDeclarationIsNotMatchToCustomsItemsInCertificate { get; } =
        new(nameof(CustomsItemInDeclarationIsNotMatchToCustomsItemsInCertificate), ErrorMessagesResources.CustomsItemInDeclarationIsNotMatchToCustomsItemsInCertificate, ValidationMessageLevel.Error);
}
#endif
