-- CRM.CertificateOfOrigins_CertificateOfOrigin: legacy columns missing from the local extraction.
-- Used by the UpdateCetrificateOfOrigins flow (attachment/feedback-message one-time guards).
-- TODO(confirm): verify column names/defaults against the monolith schema before ROLLOUT.
IF COL_LENGTH('CRM.CertificateOfOrigins_CertificateOfOrigin', 'IsCreateAttachments') IS NULL
    ALTER TABLE CRM.CertificateOfOrigins_CertificateOfOrigin
        ADD IsCreateAttachments BIT NOT NULL CONSTRAINT DF_CertificateOfOrigin_IsCreateAttachments DEFAULT (0);
GO
IF COL_LENGTH('CRM.CertificateOfOrigins_CertificateOfOrigin', 'IsMessageSent') IS NULL
    ALTER TABLE CRM.CertificateOfOrigins_CertificateOfOrigin
        ADD IsMessageSent BIT NOT NULL CONSTRAINT DF_CertificateOfOrigin_IsMessageSent DEFAULT (0);
GO
