SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
--=============================================
--Author: Tamar Hoorvitz      
--Create date: 14/11/2019
--Description Check If Exists Additional Requests For Importer.
--=============================================
CREATE OR ALTER   PROCEDURE [CRM].[usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter]
(
    @ImporterID int,
	@VendorID int=null,
	@CustomerID int=null,
	@CountryID int
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
		DECLARE @DaysForLastDelivery int = (SELECT Value FROM Infrastructure.General_enum_GlobalParam WHERE Name='AdditionalRequestsForSearchInDays')
		DECLARE @IsExistsRequests BIT;
 
		IF(@IsVendor = 1)
			BEGIN
			set @IsExistsRequests = CONVERT(BIT, IIF((	SELECT	TOP 1 1
														FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
														INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R 
														ON R.AuthenticationFileID = F.ID
														WHERE R.ImporterID = @ImporterID 
														AND R.VendorId = @VendorID
														AND F.LastDelivery > = DATEADD(DAY,(@DaysForLastDelivery * -1),GETDATE())) = 1, 1, 0))
			END
		ELSE
			BEGIN
			set @IsExistsRequests = CONVERT(BIT, IIF((	SELECT	TOP 1 1
														FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
														INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R 
														ON R.AuthenticationFileID = F.ID
														WHERE R.ImporterID = @ImporterID 
														AND R.CustomerID = @CustomerID
														AND F.LastDelivery > = DATEADD(DAY,(@DaysForLastDelivery * -1),GETDATE())) = 1, 1, 0))
			END
 
		SELECT @IsExistsRequests
 
END