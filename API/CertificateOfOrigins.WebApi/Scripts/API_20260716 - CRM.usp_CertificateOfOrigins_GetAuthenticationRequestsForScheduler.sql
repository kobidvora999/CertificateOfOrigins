SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author:           Novitsky Vitaly
-- Create date: 27/12/2016
-- Description: Creating usp_CertificateOfOrigins_GetAuthenticationRequestsForScheduler - IT 106598
-- update: 26/01/2017 Chava add Export
-- update: 11/06/2019 - 126569 - Ori - Added reminder for Vendor after 3 months
-- update: 15/10/2019 - 138666 - tamar_ho - Added parameters for Export Reminders
-- update: 30/06/2020 - 152281 - Dean_Ha - change the three month reminder
-- update: 16/03/2021 - 157790 - ruth_ha - cr 157790
-- update: 01/07/2021 - 157790 - tammar_s - cr 157924
-- update: 05/10/2021 - 161713 - ruth_ha - bug fix proc for import - customsHouse and vendor
-- update: 08/12/2021 - 157790 - tammar_s - bug 164153
-- =============================================
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestsForScheduler]
 

AS
BEGIN
       SET NOCOUNT ON;
       SET TRANSACTION      ISOLATION LEVEL READ UNCOMMITTED;
       DECLARE       @FirstReminder INT,        
                     @SecondReminder INT,
                     @ExportFirstReminder INT,
                     @ExportSecondReminder INT,
                     @IsImport BIT ,
                     @FinalDecision INT,
                     @FinalDecisionForCustomsHouse INT,
                     @TodaysDate DATETIME = GETDATE() ;

       SELECT @FirstReminder = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1600,NULL))-- 3
       SELECT @SecondReminder = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1148,NULL))-- 6
       SELECT @FinalDecision = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1941,NULL))-- 9
       SELECT @FinalDecisionForCustomsHouse = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1149,NULL))-- 10
       DECLARE @CalcFirstReminder DATE = DATEADD(MONTH, (@FirstReminder * -1), GETDATE()) 
        DECLARE @CalcSecondReminder DATE = DATEADD(MONTH, (@SecondReminder * -1), GETDATE())
       DECLARE @CalcFinalDecision DATE = DATEADD(MONTH, (@FinalDecision * -1), GETDATE())
       DECLARE @CalcFinalDecisionForCustomsHouse DATE = DATEADD(MONTH, (@FinalDecisionForCustomsHouse * -1), GETDATE())
        SELECT @ExportFirstReminder = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1667,NULL))
       SELECT @ExportSecondReminder = CONVERT(INT,Infrastructure.ufn_General_GetGlobalParamValue(1668,NULL))
        DECLARE @CalcExportFirstReminder DATE = DATEADD(MONTH, (@ExportFirstReminder * -1), GETDATE()) 
        DECLARE @CalcExportSecondtReminder DATE = DATEADD(MONTH, (@ExportSecondReminder * -1), GETDATE())
       IF OBJECT_ID('tempdb..#VendorCountryConfig') IS NOT NULL DROP TABLE #VendorCountryConfig
       -- מדינות שבהם שולחים לספק ולא לבית מכס
       SELECT *
       INTO   #VendorCountryConfig
       FROM   CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig COOCSDCC
       WHERE  COOCSDCC.[State] = 1

       --שליחת פנייה לספק – לאחר 3 חודשים מיום שליחת הפנייה לספק
       SELECT COOIAFD.ID, 
                     COOIAFD.DeliveryMethodID,
                     CAST(1 AS BIT) IsImport,
                     CAST(1 AS BIT) SendThreeMonthsReminder,
                     CAST(1 AS BIT) IsVendor,
                     COOIAR.OrganizationUnitID
       FROM   CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
                     INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE     COOIAFD.DeliveryMethodID IN (2, 3)
                     AND COOIAFD.AuthenticationFileStatusID = 2
                     AND COOIAR.VendorId IS NOT NULL
                     AND    COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID 
                                                        FROM  #VendorCountryConfig VCC)                
                     AND DATEADD(MONTH, @FirstReminder, ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) ) <= @TodaysDate
       UNION
       -- שליחת תזכורת(לבית מכס) בגין אימות מסמך העדפה -לאחר 6 חודשים מיום שליחת הפנייה
       SELECT COOIAFD.ID, 
                     COOIAFD.DeliveryMethodID,
                     CAST(1 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(0 AS BIT) IsVendor,
                     COOIAR.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
                     INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE     COOIAFD.DeliveryMethodID IN (2, 3)
                     AND COOIAFD.AuthenticationFileStatusID = 2
                     AND    COOIAR.IssuingCountryID NOT IN (SELECT   VCC.ConutryID
                                                        FROM  #VendorCountryConfig VCC)
                     AND ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) <= @CalcSecondReminder
                     AND ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) >= @CalcFinalDecisionForCustomsHouse
       UNION
       -- שליחת פנייה ליבואן – לאחר 6 חודשים מיום שליחת הפנייה לספק
       SELECT COOIAFD.ID, 
                     COOIAFD.DeliveryMethodID,
                     CAST(1 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(1 AS BIT) IsVendor,
                     COOIAR.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
                     INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE     COOIAFD.DeliveryMethodID IN (2, 3)
                     AND COOIAFD.AuthenticationFileStatusID = 3
                     AND COOIAR.VendorId IS NOT NULL
                     AND    COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID 
                                                        FROM  #VendorCountryConfig VCC)                
                     AND ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) <= @CalcSecondReminder
                     AND ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) >= @CalcFinalDecision
       UNION
       --קבלת החלטה - בתיק אמות לבית מכס זר לאחר 10 חודשים
       SELECT COOIAFD.ID,
                     COOIAFD.DeliveryMethodID,
                     CAST(1 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(0 AS BIT) IsVendor,
                     COOIAR.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
                     INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE     COOIAFD.DeliveryMethodID = 4
                     AND COOIAFD.AuthenticationFileStatusID in(2,3)
                     AND    COOIAR.IssuingCountryID NOT IN (SELECT   VCC.ConutryID
                                                        FROM  #VendorCountryConfig VCC)
                     AND CAST(ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) AS DATE) <= @CalcFinalDecisionForCustomsHouse
       UNION
       --לאחר 9 חודשים ללא החלטה של הספק - משימה קבלת החלטה סופית בתיק {0}
       SELECT COOIAFD.ID,
                     COOIAFD.DeliveryMethodID,
                     CAST(1 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(1 AS BIT) IsVendor,
                     COOIAR.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD
                     INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest COOIAR ON COOIAFD.ID = COOIAR.AuthenticationFileID
    WHERE     COOIAFD.DeliveryMethodID = 4
                     AND COOIAFD.AuthenticationFileStatusID = 3
                     AND COOIAR.VendorId IS NOT NULL
                     AND    COOIAR.IssuingCountryID IN (SELECT VCC.ConutryID 
                                                        FROM  #VendorCountryConfig VCC)  
                     AND CAST(ISNULL(COOIAFD.FirstProvideContactDate,COOIAFD.LastDelivery) AS DATE) <= @CalcFinalDecision
       UNION 
       SELECT COOIAFD.ID, 
                     COOIAFD.DeliveryMethodID,
                     CAST(0 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(0 AS BIT) IsVendor,
                     COOIAFD.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest COOIAFD
    WHERE     COOIAFD.DeliveryMethodID IN (2, 3)
                     AND COOIAFD.LastDeliveryDate <= @CalcExportFirstReminder
                     AND COOIAFD.LastDeliveryDate >= @CalcExportSecondtReminder
					 AND COOIAFD.StatusID NOT IN (6,7,8)
       UNION 
       SELECT COOIAFD.ID,
                     COOIAFD.DeliveryMethodID,
                     CAST(0 AS BIT) IsImport,
                     CAST(0 AS BIT) SendThreeMonthsReminder,
                     CAST(0 AS BIT) IsVendor,
                     COOIAFD.OrganizationUnitID
    FROM      CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest  COOIAFD
    WHERE     COOIAFD.DeliveryMethodID = 4
                     AND CAST(COOIAFD.LastDeliveryDate AS DATE) <= @CalcExportSecondtReminder 
					 AND COOIAFD.StatusID NOT IN (6,7,8)

END