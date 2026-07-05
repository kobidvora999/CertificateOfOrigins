-- Source: CRM.usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests (legacy copy, untouched)
-- Target: dbo.GetImportAuthenticationFileDetailsAndRequests - microservice-owned copy (db-proc STEP 0/6.5)
-- Generated: 2026-07-05 15:07 by db-proc audit
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
                  CAST(CASE WHEN T.ID IS NOT NULL THEN 1
                                ELSE 0 END AS BIT) IsSendReminderForImporterTaskExists,
                  DFLDSD.SubmitDate LeadDocumentSubmissionDate,
                  AR.AllInvoiceGoodsItemTaxDifference,
                  AR.InvoiceGoodsItemTaxDifference
      FROM  CRM.CertificateOfOrigins_ImportAuthenticationRequest AR
                  LEFT JOIN CRP.DealFile_LeadDocumentSubmissionData DFLDSD ON DFLDSD.LeadDocumentID = AR.LeadDocumentID
                  OUTER APPLY (     SELECT            TOP 1 T.ID
                                          FROM        Infrastructure.Tasks_Task T
                                          WHERE       T.TypeID = 404 -- שילחת תזכורת ליבואן בגין אימות מסמך {0}
                                                            AND T.TaskStatusID != 2 -- סגור
                                                            AND T.MainRelated_EntityID = AR.DocumentID
                                                            AND T.MainRelated_EntityTypeID = 12384    -- אימות מסמך מקור (יבוא)
                                          ORDER BY    T.ID) T
      WHERE AR.AuthenticationFileID = @FileID;

      SELECT      D.ID,
                  D.TypeID,
                  D.Title,
                  D.CreateDate,
                  D.ExternalIDNum,
                  D.Notes
      FROM  Infrastructure.Docs_Document D
      WHERE ID IN (       SELECT    [@DocumentIDs].val
                                FROM            @DocumentIDs);


      SELECT      I.Id,
                  I.ImportAuthenticationRequestID,
                  I.CustomItemID
      FROM  CRM.CertificateOfOrigins_ItemDetails I
      WHERE I.ImportAuthenticationRequestID IN (   SELECT   [@DocumentIDs].val
                                                                           FROM           @DocumentIDs);


END;

GO
