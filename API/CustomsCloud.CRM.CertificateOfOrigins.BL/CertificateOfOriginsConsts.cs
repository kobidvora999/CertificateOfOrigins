namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// Service constants carried over from the legacy CertificateOfOriginsConsts / CertificateOfOriginsConstants classes
// (source of truth — not invented). Kept in one place instead of scattering literals across the BL and proxies.
internal static class CertificateOfOriginsConsts
{
    // --- SaveCertificateOfOriginAttachments ---

    // Draft/Final title label + the "isDraft" sentinel checked on AdditionalInfo (CertificateOfOriginsConsts).
    public const string DraftLabel = "טיוטה";
    public const string FinalLabel = "סופי";
    public const string IsDraftSentinel = "isDraft";

    // Attachment filename format: "תעודת {type} מספר {number}.pdf" (CertificateOfOriginsConstants.CertificateName).
    public const string CertificateNameFormat = "תעודת {0} מספר {1}.pdf";

    // Only the ExportCertificateOfOrigin document type carries the certificate-number additional field
    // (EDocumentType.ExportCertificateOfOrigin + CertificateOfOriginsConsts.DocumentAdditionaFieldIDForCertificateNumber).
    public const int ExportCertificateOfOriginDocumentTypeId = 329;
    public const int CertificateNumberAdditionalFieldId = 46;

    // --- GetCertificateRequestByGuid (public-portal web query) ---

    // Legacy field labels came from SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldId), where fieldId was
    // read via reflection off the entity's [FieldID] attributes. The target DTO carries no such attributes, so the
    // verified attribute values are used as constants (source of truth: the EF4 entity — 2026-07-28).
    public const int CertificateIdToCancelFieldId = 20306;   // [FieldID] on CertificateIDToCancel
    public const int RequestReasonCodeFieldId = 20310;       // [FieldID] on RequestReasonCode
    public const int ExportDeclarationNumberFieldId = 20661; // [FieldID] on ExportDeclarationNumber

    // In-band web-query response texts (CertificateOfOriginsConsts).
    public const string IssuingDateLabel = "Issuing Date";
    public const string InvalidGuid = "Invalid Guid";
    public const string NoMatchingCertificate = "No Matching Certificate";

    // Legacy SP filter for the web-query DocumentId — the newest attached document of these types
    // (Infrastructure.Docs_Document.TypeID IN (329, 461); 329 = ExportCertificateOfOrigin).
    public static readonly int[] WebQueryDocumentTypeIds = [329, 461];

    // --- Customers proxy ---

    // ECustomerActivityType.Foreign_customs_house = 40 (בית מכס זר) — the fixed activity-type filter for
    // GetCustomersByCountry (MalamTeam.Infrastructure.GeneralServices.Environment.Enums).
    public const int ForeignCustomsHouseActivityType = 40;

    // --- SaveCertificateOfOrigin (#33) / UpdateCertificateOfOrigins (#34) ---

    // Legacy EServerTerms.CertificateUpdateRecived — appended to a superseded certificate's cancel reason.
    public const string CertificateUpdateReceived = "התקבל עדכון לתעודה"; // TODO(migration): source from ValidationMessages/resx.

    // Legacy ThereIsNoMatchBetweenTheCertificateDataAndTheDeclaration — set on a rejected reconciliation.
    public const string ReconciliationMismatchReason = "אין התאמה בין נתוני התעודה לבין ההצהרה"; // TODO(migration): source from ValidationMessages/resx.

    // The RabbitMQ exchange a published certificate is sent to for asynchronous issuing by a worker (same name as the legacy).
    public const string IssueCertificateOfOriginExchange = "IssueCertificateOfOrigin";

    // Reconciliation event AdditionalInfo cap: the exception texts are concatenated into the task field up to
    // MaximumNumberOfCharactersOfTheField, reserving LengthOfTaskStart (legacy CertificateOfOriginsConsts).
    public const int LengthOfTaskStart = 70;
    public const int MaximumNumberOfCharactersOfTheField = 253;

    // Reconciliation assessor lookup (UpdateCertificateOfOrigins warnings branch): the export lead-document entity type
    // + the Export organization-unit type (MalamTeam.Infrastructure EEntityType.ExportLeadDocument / EOrganizationUnitType.Export).
    public const int ExportLeadDocumentEntityType = 11188;
    public const int ExportOrganizationUnitType = 18;

    // --- Templates (template type ids from the legacy ETemplate / CertificateOfOriginsConstants) ---

    // EUR1 certificate-of-origin template — legacy 1-page template id 136 (report id 7000).
    public const int CertificateOfOriginEUR1TemplateTypeId = 136;
}
