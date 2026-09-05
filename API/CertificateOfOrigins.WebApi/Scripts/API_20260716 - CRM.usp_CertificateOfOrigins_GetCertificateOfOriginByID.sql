SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
 
-- Author: Tamar_Kl
 
-- Create date: 12/05/2024
-- Update date: 14/09/2025 MI 189572 update milestones
-- Update date: 29/09/2025 DevTask 189219 add IsInPublishingProcess
 
-- Description: Get Certificate Of Origin By ID
 
-- =============================================       
 
CREATE OR ALTER   PROCEDURE [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginByID]
(
	@CertificateOfOriginID INT
)
 
AS
 
BEGIN
 
	SET NOCOUNT ON;
 
	SELECT	c.ID, C.TypeID, C.Title, C.State, c.TimeStamp, C.CreateDate, C.CreateUserID, C.UpdateDate, C.UpdateUserID, C.OrganizationUnitID,
			C.OrganizationUnitID, C.CustomerID, C.CreateCustomerID, C.UpdateCustomerID, C.LeadDocumentID, C.CertificateIDToCancel, 
			C.CertificateNumber, C.CertificateOfOriginStatusID, C.DestinationCountry, C.FeedbackRemark, C.InternalApplication, 
			C.IssuingDate, C.RejectCancelReason, C.ReplacementReason, C.RequestReasonCode, C.ExportDeclarationNumber, 
			C.CertificateToReplaceInImport, C.GUID, C.QRCodePath, C.IsAttachedList, C.InSufficentworkingInd, C.InsufficentWorkingText, C.VersionNumber, C.IsLastVersion, C.ApproveUserID, C.IsInPublishingProcess	
	FROM	[CRM].[CertificateOfOrigins_CertificateOfOrigin] C
	WHERE	C.ID = @CertificateOfOriginID;
	SELECT  DE.ID, DE.CertificateOfOriginID, DE.ErrorText, DE.State
	FROM    CRM.CertificateOfOrigins_CertificateOfOriginVsDeclarationError DE
	WHERE   DE.CertificateOfOriginID = @CertificateOfOriginID
 
	SELECT  CDT.ID, CDT.Name, CDT.State, CDT.Description, CDT.EnglishName, CDT.Enumeration, CDT.StartDate, CDT.EndDate, CDT.Comment, CDT.DetailTypeFormat, CDT.DataTypeID
	FROM    CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode CDT
	SELECT	cd.ID, CD.CertificateOfOriginID, CD.CertificateDetailsTypeCodeID, CD.Value, CD.DisplayedValue
	FROM	[CRM].[CertificateOfOrigins_CertificateOfOriginDetails] CD 
	WHERE	CD.CertificateOfOriginID = @CertificateOfOriginID;
 
	SELECT ID, CertificateOfOriginID, CurrencyTypeID, InvoiceAmount, InvoiceDate, InvoiceGoodsDescription, InvoiceNumber, IsToPrint
	FROM CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail
	WHERE CertificateOfOriginID = @CertificateOfOriginID
    SELECT d.ID, d.PackingTypeID, d.CustomsItemID, d.GrossWeight, d.CertificateOfOriginInvoiceDetailID, d.ItemGoodsDescription, d.MarksAndNumbers, d.MeasurementUnitID, d.OriginCriterionID, 
	d.Quantity, d.RowNum, d.FullClassification, d.ContainerISOCode
	FROM CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail i
	JOIN CRM.CertificateOfOrigins_CertificateOfOriginItemDetail d on d.CertificateOfOriginInvoiceDetailID = i.ID
	where i.CertificateOfOriginID = @CertificateOfOriginID
 
	declare @CertificateNumber NVARCHAR(MAX)
	select @CertificateNumber = Title FROM [CRM].[CertificateOfOrigins_CertificateOfOrigin] WHERE ID = @CertificateOfOriginID;
	select COO.VersionNumber as VersionNumber,
		 --ISNULL(convert(varchar,o.IssuingDate,131),'') as 'תאריך פרסום',
		 CASE
			WHEN COO.RejectCancelReason is not null and COO.IssuingDate IS NULL AND COO.ApproveUserID IS NULL AND COO.UpdateUserID > 1000 THEN 'נדחתה'
			WHEN COO.ApproveUserID IS NOT NULL and COO.IssuingDate is not null AND COO.CertificateOfOriginStatusID = 4 THEN 'בוטלה לאחר פרסום'
			WHEN COO.ApproveUserID is  not null and COO.CertificateOfOriginStatusID <> 4 THEN 'אושרה'
		END AS ActionName,
		 COO.UpdateDate as CreateDate,
		 IIF(COO.CertificateOfOriginStatusID = 8, UA.Title, U.Title) as UserName,	 
		 isnull(COO.RejectCancelReason,'') as RejectReason		
		 from [CRM].[CertificateOfOrigins_CertificateOfOrigin] COO
			JOIN [CRM].CertificateOfOrigins_enum_CertificateOfOriginStatusCode CS ON COO.CertificateOfOriginStatusID = CS.ID
			JOIN Infrastructure.UserMng_User u ON COO.UpdateUserID = u.ID
			left JOIN Infrastructure.UserMng_User ua on ua.id = COO.ApproveUserID
	where 
	COO.Title = @CertificateNumber
	  and (
		( COO.RejectCancelReason is not null and COO.IssuingDate IS NULL AND COO.ApproveUserID IS NULL AND COO.UpdateUserID > 1000 ) -- זיהוי תעודות שנדחו ע"י עובד מכס
		OR 
		( COO.ApproveUserID is  not null and COO.CertificateOfOriginStatusID <> 4) -- תעודות שהונפקו
		OR 
		( COO.ApproveUserID IS NOT NULL and COO.IssuingDate is not null AND COO.CertificateOfOriginStatusID = 4) -- תעודות שהונפקו ובוטלו
		)
	 order by COO.ID
 
END;