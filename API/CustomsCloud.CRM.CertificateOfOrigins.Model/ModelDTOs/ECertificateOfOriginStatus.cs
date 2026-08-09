namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Certificate-of-origin status (CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode). Values are the source of
// truth from the DB C-table (verified 2026-08-06). Drives the SaveCertificateOfOrigin / UpdateCertificateOfOrigins
// status machine (cancel / validate / declaration match-mismatch / publish).
public enum ECertificateOfOriginStatus
{
    Error = 1,

    Received = 2,

    Rejected = 3,

    Cancelled = 4,

    DeclarationMismatch = 5,

    DeclarationMatch = 6,

    PendingRelease = 7,

    Published = 8,
}
