-- Source: CRM.usp_CertificateOfOrigins_GetImportAuthenticationRequestsForReminderForImporterScheduler (legacy copy, untouched).
-- Target: dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler - microservice-owned copy.
-- Drives the ReminderForImporterScheduler Planar job: import-authentication requests whose importer letter has gone
-- unanswered longer than the reminder window and that have no open reminder task yet.
--
-- Changes vs legacy:
--   * Infrastructure.ufn_General_GetGlobalParamValue(1534) replaced by the @Days parameter. No migrated dbo.* SP
--     calls that UDF - global params are service parameters now, read through IParametersUtil and passed in. The
--     matching parameter is 'DaysForReminderForImporterScheduler' (already seeded, value 45 - same as the legacy
--     comment on the UDF call).
--   * 2 cross-service JOINs removed (not owned by this service):
--       - Infrastructure.Docs_Document      (D.TypeID -> DocumentTypeID, D.Title -> DocumentTitle)
--       - Shared.Docs_enum_DocumentType     (DT.Name  -> DocumentTypeName)
--     They only supplied display fields. Nothing downstream reads them - the event the job raises is keyed on
--     DocumentID / AuthenticationFileID / OrganizationUnitID - so they are returned as 0/NULL for shape parity
--     rather than enriched through a proxy.
--   * The OUTER APPLY on Infrastructure.Tasks_Task (task type 404 SendReminderForImporter, entity type 12384, any
--     status but closed) is removed - Tasks is another service. The BL applies the same filter per row through
--     ITasksProxy.IsTaskExist, which is how the sibling reminder job already does it. NOTE this changes one
--     set-based anti-join into N proxy calls; the legacy row set is unchanged, the cost profile is not.
--   * Removed READ UNCOMMITTED.
--   * InvoiceNumber, which the legacy SELECT returns but the legacy DTO does not carry, is dropped.
CREATE OR ALTER PROCEDURE [dbo].[GetImportAuthenticationRequestsForReminderForImporterScheduler]
(
    @Days INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cross-service JOINs removed: DocumentTypeID/DocumentTypeName/DocumentTitle are placeholders (see header).
    SELECT IAR.DocumentID
         ,IAR.AuthenticationFileID
         ,IAR.LeadDocumentID
         ,CAST(0 AS INT)             DocumentTypeID
         ,CAST(NULL AS NVARCHAR(255)) DocumentTypeName
         ,CAST(NULL AS NVARCHAR(255)) DocumentTitle
         ,IAR.OrganizationUnitID
    FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest IAR
    WHERE IAR.LastDeliveryForImporter IS NOT NULL
      AND DATEDIFF(DAY, IAR.LastDeliveryForImporter, GETDATE()) > @Days
      -- 8 = letter sent to importer, 9 = reminder sent to importer
      AND IAR.DecisionID IN (8, 9)
END
