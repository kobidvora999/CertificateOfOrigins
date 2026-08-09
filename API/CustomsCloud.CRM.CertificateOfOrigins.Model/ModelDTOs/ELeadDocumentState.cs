namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Export lead-document state (owned by the ExportDealFile microservice). SaveCertificateOfOrigin treats
// Released / Paroled / Closed as "declaration released" → triggers the auto-publish reconciliation.
// TODO(blocking): these numeric values are NOT confirmed — the enum is owned by the DealFile module (not this
// service). Verify against the DealFile source/DB before ROLLOUT. Locally the ExportDealFile mock uses this same
// enum, so the release check is internally consistent.
public enum ELeadDocumentState
{
    Released = 1,

    Paroled = 2,

    Closed = 3,
}
