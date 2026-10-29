-- Source: CRM.usp_CertificateOfOrigins_GetAuthenticationRequestsForScheduler (legacy copy, untouched).
-- Target: dbo.GetAuthenticationRequestsForScheduler - microservice-owned copy.
-- Drives the AuthenticationRequestReminder Planar job: every authentication-request file (import) or export request
-- that has reached a rung of the reminder ladder. The three bit columns are what pick the event/task pair in the BL.
--
-- Changes vs legacy:
--   * The six Infrastructure.ufn_General_GetGlobalParamValue calls became parameters, matching the convention that
--     no migrated dbo.* SP reads global params directly. The BL supplies them from IParametersUtil. Mapping of the
--     legacy UDF ids to the already-seeded service parameters (all six were seeded with no consumer - they were put
--     there for exactly this job):
--         1600 @FirstReminder                 -> DaysForFirstReminderInAuthenticationRequest3        (3, matches the legacy comment)
--         1148 @SecondReminder                -> DaysForFirstReminderInAuthenticationRequest1        (6, matches)
--         1941 @FinalDecision                 -> MonthForFinalReminderInAuthenticationRequest        (9, matches)
--         1149 @FinalDecisionForCustomsHouse  -> DaysForFirstReminderInAuthenticationRequest2        (10, matches)
--         1667 @ExportFirstReminder           -> DaysForFirstReminderInExportAuthenticationRequest1  (6)
--         1668 @ExportSecondReminder          -> DaysForSecondReminderInExportAuthenticationRequest2 (10)
--     All six are MONTH offsets despite four of them being named "Days..." in the seed - the arithmetic below is
--     DATEADD(MONTH, ...) exactly as in the legacy SP.
--   * No JOINs removed: every table this SP reads is owned by this service.
--   * Removed READ UNCOMMITTED. The #VendorCountryConfig temp table is kept as-is (it is a plain filter on the
--     service's own SupplierDeliveryCountryConfig).
CREATE OR ALTER PROCEDURE [dbo].[GetAuthenticationRequestsForScheduler]
(
    @FirstReminder                INT,
    @SecondReminder               INT,
    @FinalDecision                INT,
    @FinalDecisionForCustomsHouse INT,
    @ExportFirstReminder          INT,
    @ExportSecondReminder         INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TodaysDate DATETIME = GETDATE();

    DECLARE @CalcSecondReminder               DATE = DATEADD(MONTH, (@SecondReminder * -1), GETDATE());
    DECLARE @CalcFinalDecision                DATE = DATEADD(MONTH, (@FinalDecision * -1), GETDATE());
    DECLARE @CalcFinalDecisionForCustomsHouse DATE = DATEADD(MONTH, (@FinalDecisionForCustomsHouse * -1), GETDATE());
    DECLARE @CalcExportFirstReminder          DATE = DATEADD(MONTH, (@ExportFirstReminder * -1), GETDATE());
    DECLARE @CalcExportSecondReminder         DATE = DATEADD(MONTH, (@ExportSecondReminder * -1), GETDATE());

    IF OBJECT_ID('tempdb..#VendorCountryConfig') IS NOT NULL DROP TABLE #VendorCountryConfig;

    -- Countries whose letters go to the supplier rather than to the customs house.
    SELECT *
    INTO   #VendorCountryConfig
    FROM   CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig COOCSDCC
    WHERE  COOCSDCC.[State] = 1;

    -- Letter to the supplier - 3 months after the supplier letter went out.
    SELECT COOIAFD.ID,
           COOIAFD.DeliveryMethodID,
           CAST(1 AS BIT) IsImport,
           CAST(1 AS BIT) SendThreeMonthsReminder,
           CAST(1 AS BIT) IsVendor,
           COOIAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
           INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE  COOIAFD.DeliveryMethodID IN (2, 3)
      AND  COOIAFD.AuthenticationFileStatusID = 2
      AND  COOIAR.VendorId IS NOT NULL
      AND  COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID FROM #VendorCountryConfig VCC)
      AND  DATEADD(MONTH, @FirstReminder, ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery)) <= @TodaysDate

    UNION
    -- Reminder to the customs house for a preference-document verification - 6 months after the letter went out.
    SELECT COOIAFD.ID,
           COOIAFD.DeliveryMethodID,
           CAST(1 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(0 AS BIT) IsVendor,
           COOIAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
           INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE  COOIAFD.DeliveryMethodID IN (2, 3)
      AND  COOIAFD.AuthenticationFileStatusID = 2
      AND  COOIAR.IssuingCountryID NOT IN (SELECT VCC.ConutryID FROM #VendorCountryConfig VCC)
      AND  ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) <= @CalcSecondReminder
      AND  ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) >= @CalcFinalDecisionForCustomsHouse

    UNION
    -- Letter to the importer - 6 months after the supplier letter went out.
    SELECT COOIAFD.ID,
           COOIAFD.DeliveryMethodID,
           CAST(1 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(1 AS BIT) IsVendor,
           COOIAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
           INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE  COOIAFD.DeliveryMethodID IN (2, 3)
      AND  COOIAFD.AuthenticationFileStatusID = 3
      AND  COOIAR.VendorId IS NOT NULL
      AND  COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID FROM #VendorCountryConfig VCC)
      AND  ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) <= @CalcSecondReminder
      AND  ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) >= @CalcFinalDecision

    UNION
    -- Decision due on a foreign-customs-house verification file - after 10 months.
    SELECT COOIAFD.ID,
           COOIAFD.DeliveryMethodID,
           CAST(1 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(0 AS BIT) IsVendor,
           COOIAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
           INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE  COOIAFD.DeliveryMethodID = 4
      AND  COOIAFD.AuthenticationFileStatusID IN (2, 3)
      AND  COOIAR.IssuingCountryID NOT IN (SELECT VCC.ConutryID FROM #VendorCountryConfig VCC)
      AND  CAST(ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) AS DATE) <= @CalcFinalDecisionForCustomsHouse

    UNION
    -- 9 months with no supplier decision - open the "final decision in the file" task.
    SELECT COOIAFD.ID,
           COOIAFD.DeliveryMethodID,
           CAST(1 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(1 AS BIT) IsVendor,
           COOIAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
           INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE  COOIAFD.DeliveryMethodID = 4
      AND  COOIAFD.AuthenticationFileStatusID = 3
      AND  COOIAR.VendorId IS NOT NULL
      AND  COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID FROM #VendorCountryConfig VCC)
      AND  CAST(ISNULL(COOIAFD.FirstProvideContactDate, COOIAFD.LastDelivery) AS DATE) <= @CalcFinalDecision

    UNION
    -- Export: first reminder window.
    SELECT COOEDAR.ID,
           COOEDAR.DeliveryMethodID,
           CAST(0 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(0 AS BIT) IsVendor,
           COOEDAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest COOEDAR
    WHERE  COOEDAR.DeliveryMethodID IN (2, 3)
      AND  COOEDAR.LastDeliveryDate <= @CalcExportFirstReminder
      AND  COOEDAR.LastDeliveryDate >= @CalcExportSecondReminder
      AND  COOEDAR.StatusID NOT IN (6, 7, 8)

    UNION
    -- Export: second reminder window.
    SELECT COOEDAR.ID,
           COOEDAR.DeliveryMethodID,
           CAST(0 AS BIT) IsImport,
           CAST(0 AS BIT) SendThreeMonthsReminder,
           CAST(0 AS BIT) IsVendor,
           COOEDAR.OrganizationUnitID
    FROM   CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest COOEDAR
    WHERE  COOEDAR.DeliveryMethodID = 4
      AND  CAST(COOEDAR.LastDeliveryDate AS DATE) <= @CalcExportSecondReminder
      AND  COOEDAR.StatusID NOT IN (6, 7, 8)
END
