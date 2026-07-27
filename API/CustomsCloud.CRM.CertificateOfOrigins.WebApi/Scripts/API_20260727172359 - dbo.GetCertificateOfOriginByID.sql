-- =============================================
-- dbo.GetCertificateOfOriginByID (service copy of CRM.usp_CertificateOfOrigins_GetCertificateOfOriginByID)
-- Single certificate-of-origin with its full graph via 7 result sets:
--   1 header · 2 declaration errors · 3 detail-type-code lookup · 4 details · 5 invoices · 6 invoice items · 7 milestones
-- Migration changes vs. the monolith SP (method #17 GetCertificateOfOriginById):
--   - result set 7: cross-service JOINs to Infrastructure.UserMng_User removed (table not in this service DB).
--     The acting user id is returned raw (UserId = IIF(status=8, ApproveUserID, UpdateUserID)); the user's
--     display name is enriched in the BL via IUserProxy.
--   - result set 7: Hebrew ActionName literals given the N prefix (nvarchar) so they survive a non-Hebrew code page.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetCertificateOfOriginByID]
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

	-- Milestones. Cross-service Infrastructure.UserMng_User JOINs removed (table not in this service DB);
	-- the acting user id is returned raw (IIF: approve-user on status 8, else update-user) and its name is
	-- enriched in the BL via IUserProxy. Hebrew ActionName literals carry the N prefix (nvarchar).
	select COO.VersionNumber as VersionNumber,
		 CASE
			WHEN COO.RejectCancelReason is not null and COO.IssuingDate IS NULL AND COO.ApproveUserID IS NULL AND COO.UpdateUserID > 1000 THEN N'נדחתה'
			WHEN COO.ApproveUserID IS NOT NULL and COO.IssuingDate is not null AND COO.CertificateOfOriginStatusID = 4 THEN N'בוטלה לאחר פרסום'
			WHEN COO.ApproveUserID is  not null and COO.CertificateOfOriginStatusID <> 4 THEN N'אושרה'
		END AS ActionName,
		 COO.UpdateDate as CreateDate,
		 IIF(COO.CertificateOfOriginStatusID = 8, COO.ApproveUserID, COO.UpdateUserID) as UserId,
		 isnull(COO.RejectCancelReason,'') as RejectReason
		 from [CRM].[CertificateOfOrigins_CertificateOfOrigin] COO
			JOIN [CRM].CertificateOfOrigins_enum_CertificateOfOriginStatusCode CS ON COO.CertificateOfOriginStatusID = CS.ID
	where
	COO.Title = @CertificateNumber
	  and (
		( COO.RejectCancelReason is not null and COO.IssuingDate IS NULL AND COO.ApproveUserID IS NULL AND COO.UpdateUserID > 1000 )
		OR
		( COO.ApproveUserID is  not null and COO.CertificateOfOriginStatusID <> 4)
		OR
		( COO.ApproveUserID IS NOT NULL and COO.IssuingDate is not null AND COO.CertificateOfOriginStatusID = 4)
		)
	 order by COO.ID

END;
