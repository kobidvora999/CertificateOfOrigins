---- CRM.CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest ----
-- The customs items of an export authentication request (MergeExportDocumentAuthenticationRequestChildren writes
-- them). Missing from API_20260715 - create tables.sql; the definition is the legacy one
-- (CertificateOfOriginsObjectModel.edmx) with the constraint names of the existing databases. Idempotent.
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExportDocumentAuthenticationRequestID] [int] NOT NULL,
	[CustomsItemID] [int] NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest] PRIMARY KEY CLUSTERED ([ID] ASC),
CONSTRAINT [UQ_CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest] UNIQUE NONCLUSTERED
    ([ExportDocumentAuthenticationRequestID] ASC, [CustomsItemID] ASC)
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest] WITH CHECK
    ADD CONSTRAINT [FK_crm_CustomsItemToExportDocumentAuthenticationRequest_CertificateOfOrigins_ExportDocumentAuthenticationRequest]
    FOREIGN KEY ([ExportDocumentAuthenticationRequestID])
    REFERENCES [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] ([ID])
END
