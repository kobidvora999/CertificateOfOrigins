namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Export lead-document state (owned by the ExportDealFile microservice). Values are the authoritative
// CertificateOfOrigins_enum_LeadDocumentState / DealFile lead-document-state C-table ids (confirmed 2026-08).
// SaveCertificateOfOrigin treats Submited / Released / Paroled / Closed as "declaration released" → triggers the
// auto-publish reconciliation; GetPC_MSG2280_2281 CheckExportDeclarationNumber rejects Draft / CanceledDraft / Canceled.
public enum ELeadDocumentState
{
    Draft = 1,

    Submited = 2,

    Released = 3,

    Paroled = 4,

    CanceledDraft = 5,

    Canceled = 6,

    Splitted = 7,

    WaitingToEnter = 8,

    WaitingToExit = 9,

    TrsnsitProhibted = 12,

    Closed = 13,

    Confiscated = 14,

    PermittedToEnter = 15,

    PermittedToExit = 16,

    ReleasedToEnter = 17,

    ReleasedToExit = 18,
}
