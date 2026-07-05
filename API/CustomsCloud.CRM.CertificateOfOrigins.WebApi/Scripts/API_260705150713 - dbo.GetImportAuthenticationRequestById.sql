-- Source: CRM.usp_CertificateOfOrigins_CertificateOfOrigins_GetImportAuthenticationRequestById (legacy copy, untouched)
-- Target: dbo.GetImportAuthenticationRequestById - microservice-owned copy (db-proc STEP 0/6.5)
-- Generated: 2026-07-05 15:07 by db-proc audit
-- =============================================
-- Author: Chava
-- Create date: 30/11/2016
-- Description:  GetAuthenticationRequestFile And his children by fileID
-- UPDATE : 08/03/2017 Chava add customerID
-- UPDATE : 07/09/2017 Chava add ImporterID
-- UPDATE : 04/06/2019 CR126551 - Ori K. - Add InvoiceNumber
-- Update date: 14/07/2019 Bug 136359 Assaf Perl
-- UPDATE : 24/03/2021 CR156286 - ruth_ha add InvoiceGoodsItemTaxDifference   , AllInvoiceGoodsItemTaxDifference
-- =============================================      
CREATE OR ALTER PROCEDURE [dbo].[GetImportAuthenticationRequestById]
(
      --declare
      @DocumentID INT
)
AS
BEGIN
      SET NOCOUNT ON;
      SELECT      [DocumentID],
                  [CreateDate],
                  [CreateUserID],
                  [UpdateDate],
                  [UpdateUserID],
                  [AuthenticationFileID],
                  [AuthenticationRequestDate],
                  [CirumstanceDetails],
                  [CollateralID],
                  [DecisionCircumstences],
                  [DecisionID],
                  R.[LeadDocumentID],
                  [DocumentIssuingDate],
                  [ImportCountryID],
                  [IssuingCountryID],
                  [ItemDetailID],
                  [Number],
                  [IsOldIndication],
                  [OriginCountryID],
                  [PreferenceDocumentTypeID],
                  [Remarks],
                  [RequestCircumstancesID],
                  [UserResponseID],
                  [ResponseNameEmail],
                  [ResponsePhoneNum],
                  [OrganizationUnitID],
                  [UserID],
                  [VendorId],
                  [VendorName],
                  [OrganizationUnitTypeID],
                  [DocumentNumber],
                  R.CustomerID,
                  R.ImporterID,
                  R.InvoiceNumber,
                  R.InvoiceGoodsItemTaxDifference     ,
                  R.AllInvoiceGoodsItemTaxDifference,
                  CAST(NULL AS DATETIME) LeadDocumentSubmissionDate -- db-proc Pattern F: CRP.DealFile_* not replicated (no DealFile microservice yet)
      FROM  [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] R
                  -- LEFT JOIN CRP.DealFile_LeadDocumentSubmissionData DFLDSD ON DFLDSD.LeadDocumentID = R.LeadDocumentID  (not replicated)
                  
      WHERE R.DocumentID = @DocumentID;

      SELECT      [Id],
                  [ImportAuthenticationRequestID],
                  [CustomItemID]
      FROM  [CRM].[CertificateOfOrigins_ItemDetails] I
      WHERE I.ImportAuthenticationRequestID = @DocumentID;



      SELECT      D.ID,
                  D.TypeID,
                  D.Title,
                  D.CreateDate,
                  D.ExternalIDNum,
                  D.Notes
      FROM (SELECT CAST(NULL AS INT) ID, CAST(NULL AS INT) TypeID, CAST(NULL AS NVARCHAR(255)) Title, CAST(NULL AS DATETIME) CreateDate, CAST(NULL AS NVARCHAR(50)) ExternalIDNum, CAST(NULL AS NVARCHAR(MAX)) Notes) D -- db-proc Pattern F: Infrastructure.Docs_Document not replicated (Documents microservice)
      WHERE 1 = 0;
END;

GO
