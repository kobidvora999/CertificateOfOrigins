SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author: Efrat_Y
-- Create date: 01/04/2019 - CR 126572
-- Update date: 04/04/2019
-- Description:  GetImportAuthenticationRequestsForReminderForImporterScheduler
-- Update: 16/06/2019 - Ori - Added InvoiceNumber
-- =============================================       
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetImportAuthenticationRequestsForReminderForImporterScheduler]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SET NOCOUNT ON;
 
	DECLARE @DaysForReminderForImporterScheduler INT = CONVERT(INT, Infrastructure.ufn_General_GetGlobalParamValue(1534, NULL)); -- 45
	SELECT IAR.DocumentID
			,IAR.AuthenticationFileID
			,IAR.LeadDocumentID
			,D.TypeID DocumentTypeID
			,DT.Name DocumentTypeName
			,D.Title DocumentTitle
			,IAR.OrganizationUnitID
			,IAR.InvoiceNumber
	FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest IAR
	INNER JOIN Infrastructure.Docs_Document D ON D.ID = IAR.DocumentID
	INNER JOIN Shared.Docs_enum_DocumentType DT ON DT.ID = D.TypeID
	OUTER APPLY
	(
		SELECT TOP 1 T.ID
		FROM Infrastructure.Tasks_Task T
		WHERE T.TypeID = 404 -- שילחת תזכורת ליבואן בגין אימות מסמך {0}
		AND T.TaskStatusID != 2 -- סגור 
		AND T.MainRelated_EntityID = IAR.DocumentID
		AND T.MainRelated_EntityTypeID = 12384 -- אימות מסמך מקור (יבוא) 
	)T
	WHERE LastDeliveryForImporter IS NOT NULL
	AND DATEDIFF(DAY, LastDeliveryForImporter, GETDATE()) > @DaysForReminderForImporterScheduler
	AND (IAR.DecisionID = 8 -- נשלחה פנייה ליבואן
			OR IAR.DecisionID = 9 -- נשלחה תזכורת ליבואן
		)
	AND T.ID IS NULL
END