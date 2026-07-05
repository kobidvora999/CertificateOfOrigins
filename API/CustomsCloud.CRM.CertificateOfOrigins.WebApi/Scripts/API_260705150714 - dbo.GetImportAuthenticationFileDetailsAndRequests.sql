IF SCHEMA_ID('Shared') IS NULL EXEC('CREATE SCHEMA Shared');
GO
IF TYPE_ID('Shared.IntArray') IS NULL
    CREATE TYPE Shared.IntArray AS TABLE ([val] INT);
GO

CREATE OR ALTER PROCEDURE [dbo].[GetImportAuthenticationFileDetailsAndRequests]
(
      --declare
      @FileID INT
)
AS
BEGIN

      SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
      SET NOCOUNT ON;

      SELECT      C.[ID],
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
      FROM  CRM.CertificateOfOrigins_ImportAuthenticationFileDetails C
      WHERE C.ID = @FileID;

      DECLARE @DocumentIDs Shared.IntArray;

      INSERT      @DocumentIDs
      SELECT      AR.DocumentID
      FROM  CRM.CertificateOfOrigins_ImportAuthenticationRequest AR
      WHERE AR.AuthenticationFileID = @FileID;

      SELECT      AR.DocumentID,
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
                  CAST(0 AS BIT) IsSendReminderForImporterTaskExists, -- db-proc Pattern F: Infrastructure.Tasks_Task not replicated (Tasks microservice) ג€” enrich in BL if needed

                  CAST(NULL AS DATETIME) LeadDocumentSubmissionDate, -- CRP.DealFile_* not replicated (no DealFile microservice)
                  AR.AllInvoiceGoodsItemTaxDifference,
                  AR.InvoiceGoodsItemTaxDifference
      FROM  CRM.CertificateOfOrigins_ImportAuthenticationRequest AR








      WHERE AR.AuthenticationFileID = @FileID;

      SELECT      D.ID,
                  D.TypeID,
                  D.Title,
                  D.CreateDate,
                  D.ExternalIDNum,
                  D.Notes
      FROM (SELECT CAST(NULL AS INT) ID, CAST(NULL AS INT) TypeID, CAST(NULL AS NVARCHAR(255)) Title, CAST(NULL AS DATETIME) CreateDate, CAST(NULL AS NVARCHAR(50)) ExternalIDNum, CAST(NULL AS NVARCHAR(MAX)) Notes) D -- Docs_Document not replicated (Documents microservice)
      WHERE 1 = 0;



      SELECT      I.Id,
                  I.ImportAuthenticationRequestID,
                  I.CustomItemID
      FROM  CRM.CertificateOfOrigins_ItemDetails I
      WHERE I.ImportAuthenticationRequestID IN (   SELECT   [@DocumentIDs].val
                                                                           FROM           @DocumentIDs);


END;

GO
