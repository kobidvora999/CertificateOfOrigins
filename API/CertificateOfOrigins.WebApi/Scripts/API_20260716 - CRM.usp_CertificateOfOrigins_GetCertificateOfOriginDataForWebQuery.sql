SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author:		Assaf Perl
-- Create date: 16/09/2019
-- Description:	Get CertificateOfOrigin Data For Web Query
-- Task:		CR 129425
-- 09/03/2020 Dean Harush - Taking displayed value instead of value.
-- Update Date : 23/03/2023 Tchiya_s Add Two parameters
-- Update Date : 30/01/2024 tammar_s Change documentType 461,329
-- Update Date : 21/07/2024 Tamar_Kl FIX BUG 183195
-- =============================================
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginDataForWebQuery] 
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
            CREATE TABLE #CertificateDetailsTypeCodeForWebDisplay 
            (
                        CertificateDetailsTypeCode INT NOT NULL PRIMARY KEY,
                        Label NVARCHAR(255) NOT NULL,
                        Value NVARCHAR(255) NOT NULL,
                        IsToPrint BIT NOT NULL
            )
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
 
            SELECT top 1   @DocumentID = DD.ID 
            FROM   Infrastructure.Docs_EntityDocument DED
                                    INNER JOIN Infrastructure.Docs_Document DD ON DD.ID = DED.DocumentID
            WHERE DED.EntityID = @CertificateOfOriginID
                                    AND     DD.TypeID in (329, 461)
                                    AND DED.EntitytypeID =12319
			ORDER BY DD.CreateDate DESC
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