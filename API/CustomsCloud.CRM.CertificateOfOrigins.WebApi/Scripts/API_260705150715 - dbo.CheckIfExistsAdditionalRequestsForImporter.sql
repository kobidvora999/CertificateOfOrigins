-- Source: CRM.usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter (legacy copy, untouched)
-- Target: dbo.CheckIfExistsAdditionalRequestsForImporter - microservice-owned copy (db-proc STEP 0/6.5)
-- Generated: 2026-07-05 15:07 by db-proc audit
 --=============================================
 --Author: Tamar Hoorvitz      
 --Create date: 14/11/2019
 --Description Check If Exists Additional Requests For Importer.
 --=============================================
CREATE OR ALTER PROCEDURE [dbo].[CheckIfExistsAdditionalRequestsForImporter]
 (
    @ImporterID int,
      @VendorID int=null,
      @CustomerID int=null,
      @CountryID int,
      @DaysForLastDelivery int
 )
AS
    BEGIN   
        SET NOCOUNT ON;

            --DECLARE   
            --@ImporterID int,
            --@VendorID int,
            --@CustomerID int,
            --@CountryID int

            DECLARE @IsVendor BIT = CONVERT(BIT, IIF((SELECT TOP 1 1 FROM CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig WHERE ConutryID = @CountryID) = 1, 1, 0))
            -- db-proc Pattern B: Infrastructure.General_enum_GlobalParam is not replicated - value supplied by caller (IParametersUtil)
            -- DECLARE @DaysForLastDelivery int = (SELECT Value FROM Infrastructure.General_enum_GlobalParam WHERE Name='AdditionalRequestsForSearchInDays')
            DECLARE @IsExistsRequests BIT;

            IF(@IsVendor = 1)
                  BEGIN
                  set @IsExistsRequests = CONVERT(BIT, IIF((      SELECT      TOP 1 1
                                                                                    FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
                                                                                    INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R
                                                                                    ON R.AuthenticationFileID = F.ID
                                                                                    WHERE R.ImporterID = @ImporterID
                                                                                    AND R.VendorId = @VendorID
                                                                                    AND F.LastDelivery > = DATEADD(DAY,(@DaysForLastDelivery * -1),GETDATE())) = 1, 1, 0))
                  END
                                                                        
            ELSE
                  BEGIN
                  set @IsExistsRequests = CONVERT(BIT, IIF((      SELECT      TOP 1 1
                                                                                    FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
                                                                                    INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R
                                                                                    ON R.AuthenticationFileID = F.ID
                                                                                    WHERE R.ImporterID = @ImporterID
                                                                                    AND R.CustomerID = @CustomerID
                                                                                    AND F.LastDelivery > = DATEADD(DAY,(@DaysForLastDelivery * -1),GETDATE())) = 1, 1, 0))
                  END

            SELECT @IsExistsRequests

END

GO
