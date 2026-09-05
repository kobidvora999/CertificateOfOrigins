-- =============================================
-- dbo.GetCertificateOfOriginDataForWebQuery (service copy of CRM.usp_CertificateOfOrigins_GetCertificateOfOriginDataForWebQuery)
-- Certificate verification for the public portal (GetPC_Web_9096_CertificateRequest), by guid or by
-- CertificateOfOriginNumber + IssuingDate. Returns 5 result sets:
--   1 header · 2 invoices · 3 details · 4 detail-type-code lookup · 5 web print-out
-- Migration changes vs. the monolith SP (Incoming method GetCertificateRequestByGuid):
--   - DocumentID: the cross-service JOIN to Infrastructure.Docs_EntityDocument / Infrastructure.Docs_Document
--     (Documents service, not owned by this module) was removed — the tables are not in this service DB.
--     @DocumentID is returned as NULL; resolve it via the Documents service (TODO(blocking) in the BL).
--   - The dead #CertificateDetailsTypeCodeForWebDisplay temp table (created but never populated or selected in
--     the monolith) was dropped — it had no effect on any result set.
-- Preserved legacy quirk: result set 5 does NOT return an IsToPrint column (so the BL's
-- CertificateDetailsTypeIsToPrint is always false — Consignee fields for EUR1/EURMED are never printed).
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetCertificateOfOriginDataForWebQuery]
            @Guid UNIQUEIDENTIFIER,
            @CertificateOfOriginNumber NVARCHAR(35),
            @IssuingDate DATE
AS
BEGIN
            SET NOCOUNT ON;
            SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

            DECLARE @CertificateOfOriginID INT;
            DECLARE @CertificateOfOriginTypeID INT;
            DECLARE @DocumentID INT;

            CREATE TABLE #CertificateOfOriginDetails ( CertificateOfOriginDetailsID INT NOT NULL PRIMARY KEY)

            IF @Guid IS NOT NULL
            BEGIN
                SELECT  @CertificateOfOriginID = COOCOO.ID ,
                        @CertificateOfOriginTypeID = COOCOO.TypeID
                FROM   CRM.CertificateOfOrigins_CertificateOfOrigin COOCOO
                WHERE COOCOO.GUID = @Guid
            END
            ELSE
            BEGIN
                SELECT  @CertificateOfOriginID = COOCOO.ID ,
                        @CertificateOfOriginTypeID = COOCOO.TypeID
                FROM   CRM.CertificateOfOrigins_CertificateOfOrigin COOCOO
                WHERE COOCOO.CertificateNumber = @CertificateOfOriginNumber
                      AND CONVERT(DATE,COOCOO.IssuingDate) = @IssuingDate
            END

            -- TODO(blocking): DocumentID was resolved from Infrastructure.Docs_* (Documents service, cross-schema,
            -- not owned by this module). Returned NULL for now; resolve via the Documents service.
            SET @DocumentID = NULL;

            SELECT  COOCOO.ID,
                    COOCOO.TypeID,
                    COOCOO.Title,
                    COOCOO.State,
                    COOCOO.TimeStamp,
                    COOCOO.CreateDate,
                    COOCOO.CreateUserID,
                    COOCOO.UpdateDate,
                    COOCOO.UpdateUserID,
                    COOCOO.OrganizationUnitID,
                    COOCOO.CustomerID,
                    COOCOO.CreateCustomerID,
                    COOCOO.UpdateCustomerID,
                    COOCOO.LeadDocumentID,
                    COOCOO.CertificateIDToCancel,
                    COOCOO.CertificateNumber,
                    COOCOO.CertificateOfOriginStatusID,
                    COOCOO.DestinationCountry,
                    COOCOO.FeedbackRemark,
                    COOCOO.InternalApplication,
                    COOCOO.IssuingDate,
                    COOCOO.RejectCancelReason,
                    COOCOO.ReplacementReason,
                    COOCOO.RequestReasonCode,
                    COOCOO.ExportDeclarationNumber,
                    COOCOO.CertificateToReplaceInImport,
                    COOCOO.GUID,
                    COOCOO.QRCodePath,
                    @DocumentID DocumentID
            FROM   CRM.CertificateOfOrigins_CertificateOfOrigin COOCOO
            WHERE COOCOO.ID = @CertificateOfOriginID

            SELECT  COOCOOID.ID,
                    COOCOOID.CertificateOfOriginID,
                    COOCOOID.CurrencyTypeID,
                    COOCOOID.InvoiceAmount,
                    COOCOOID.InvoiceDate,
                    COOCOOID.InvoiceGoodsDescription,
                    COOCOOID.InvoiceNumber,
                    COOCOOID.IsToPrint
            FROM   CRM.CertificateOfOrigins_CertificateOfOriginInvoiceDetail COOCOOID
            WHERE COOCOOID.CertificateOfOriginID = @CertificateOfOriginID

            INSERT #CertificateOfOriginDetails
            (
                        CertificateOfOriginDetailsID
            )
            SELECT  ID
            FROM   CRM.CertificateOfOrigins_CertificateOfOriginDetails COOCOOD
            WHERE COOCOOD.CertificateOfOriginID = @CertificateOfOriginID

            SELECT  COOCOOD.ID,
                    COOCOOD.CertificateOfOriginID,
                    COOCOOD.CertificateDetailsTypeCodeID,
                    COOCOOD.Value,
                    COOCOOD.DisplayedValue
            FROM   CRM.CertificateOfOrigins_CertificateOfOriginDetails COOCOOD
                    INNER JOIN #CertificateOfOriginDetails COOD ON COOCOOD.ID = COOD.CertificateOfOriginDetailsID

            SELECT  COOECDTC.ID,
                    COOECDTC.Name,
                    COOECDTC.State,
                    COOECDTC.Description,
                    COOECDTC.EnglishName,
                    COOECDTC.Enumeration,
                    COOECDTC.StartDate,
                    COOECDTC.EndDate,
                    COOECDTC.Comment,
                    COOECDTC.DetailTypeFormat
            FROM   CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode COOECDTC

            SELECT  COOECDTC.ID CertificateDetailsTypeID,
                    COOECDTC.EnglishName CertificateDetailsTypeEnglishName,
                    COOCOOD.DisplayedValue CertificateDetailsTypeValue
            FROM   CRM.CertificateOfOrigins_enum_CertificateDetailsTypeCode COOECDTC
                    INNER JOIN CRM.CertificateOfOrigins_CertificateOfOriginDetails COOCOOD ON COOCOOD.CertificateDetailsTypeCodeID = COOECDTC.ID
                    INNER JOIN #CertificateOfOriginDetails COOD ON COOCOOD.ID = COOD.CertificateOfOriginDetailsID
END
