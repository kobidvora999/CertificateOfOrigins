SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author: Chava
-- Create date: 30/11/2016
-- Update date: 28/02/2017 add AR.CustomerID
-- Update date: 02/04/2019 Efrat_Y - CR 126572 add AR.ImporterID, AR.LastDeliveryForImporter, IsSendReminderForImporterTaskExists
-- Description:  GetAuthenticationRequestFile And his children by fileID
-- Update date: 04/06/2019 CR126551 - Ori K. - Add InvoiceNumber
-- update date: 26/07/2020 BUG 153145 - Dean Ha - deleted MeaserType , MeasurementUnitId , GoodsDescription
-- update date: 23/06/2021 - tammar_s add ImporterContactingReasonID
-- update date: 04/11/2021 - tammar_s add AllInvoiceGoodsItemTaxDifference and InvoiceGoodsItemTaxDifference
-- =============================================       
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests]
(
	--declare
	@FileID INT
)
AS
BEGIN
 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SET NOCOUNT ON;
 
	SELECT	C.[ID],
			C.[State],
			C.[TimeStamp],
			C.[CreateDate],
			C.[CreateUserID],
			C.[UpdateDate],
			C.[UpdateUserID],
			C.[AuthenticationFileStatusID],
			C.[Notes],
			C.[PostalAdress],
			C.[DeliveryMethodID],
			C.[EmailAdress],
			C.[ReminderMethodID],
			C.[RequestCountryID],
			C.[UserID],
			C.[UserNameIssuingLetter],
			C.LastDelivery,
			C.ImporterContactingReasonID,
			C.FirstProvideContactDate
	FROM	CRM.CertificateOfOrigins_ImportAuthenticationFileDetails C
	WHERE	C.ID = @FileID;
 
	DECLARE @DocumentIDs Shared.IntArray;
 
	INSERT	@DocumentIDs
	SELECT	AR.DocumentID
	FROM	CRM.CertificateOfOrigins_ImportAuthenticationRequest AR
	WHERE	AR.AuthenticationFileID = @FileID;
 
	SELECT	AR.DocumentID,
			AR.CreateDate,
			AR.CreateUserID,
			AR.AuthenticationFileID,
			AR.AuthenticationRequestDate,
			AR.CirumstanceDetails,
			AR.DecisionCircumstences,
			AR.DecisionCircumstences,
			AR.DecisionID,
			AR.LeadDocumentID,
			AR.DocumentIssuingDate,
			AR.ImportCountryID,
			AR.IssuingCountryID,
			AR.ItemDetailID,
			AR.IsOldIndication,
			AR.OriginCountryID,
			AR.PreferenceDocumentTypeID,
			AR.Remarks,
			AR.UserResponseID,
			AR.ResponseNameEmail,
			AR.ResponsePhoneNum,
			AR.OrganizationUnitID,
			AR.UserID,
			AR.VendorId,
			AR.OrganizationUnitTypeID,
			AR.DocumentNumber,
			AR.RequestCircumstancesID,
			AR.CustomerID,
			AR.ImporterID,
			AR.LastDeliveryForImporter,
			AR.InvoiceNumber,
			CAST(CASE WHEN T.ID IS NOT NULL THEN 1
					  ELSE 0 END AS BIT) IsSendReminderForImporterTaskExists,
			DFLDSD.SubmitDate LeadDocumentSubmissionDate,
			AR.AllInvoiceGoodsItemTaxDifference,
			AR.InvoiceGoodsItemTaxDifference
	FROM	CRM.CertificateOfOrigins_ImportAuthenticationRequest AR
			LEFT JOIN CRP.DealFile_LeadDocumentSubmissionData DFLDSD ON DFLDSD.LeadDocumentID = AR.LeadDocumentID
			OUTER APPLY (	SELECT		TOP 1	T.ID
							FROM		Infrastructure.Tasks_Task T
							WHERE		T.TypeID = 404 -- שילחת תזכורת ליבואן בגין אימות מסמך {0}
										AND T.TaskStatusID != 2 -- סגור 
										AND T.MainRelated_EntityID = AR.DocumentID
										AND T.MainRelated_EntityTypeID = 12384	-- אימות מסמך מקור (יבוא) 
							ORDER BY	T.ID) T
	WHERE	AR.AuthenticationFileID = @FileID;
 
	SELECT	D.ID,
			D.TypeID,
			D.Title,
			D.CreateDate,
			D.ExternalIDNum,
			D.Notes
	FROM	Infrastructure.Docs_Document D
	WHERE	ID IN (	  SELECT	[@DocumentIDs].val
					  FROM		@DocumentIDs);
 
 
	SELECT	I.Id,
			I.ImportAuthenticationRequestID,
			I.CustomItemID
	FROM	CRM.CertificateOfOrigins_ItemDetails I
	WHERE	I.ImportAuthenticationRequestID IN (   SELECT	[@DocumentIDs].val
												   FROM		@DocumentIDs);
 
 
END;