-- Source: CRM.usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter (legacy copy, untouched)
-- Target: dbo.CheckIfExistsAdditionalRequestsForImporter - microservice-owned copy.
-- Only change vs legacy: the @DaysForLastDelivery config read is redirected from the platform table
-- Infrastructure.General_enum_GlobalParam (not present in the service DB) to the local Infrastructure.Parameters
-- table (the service's own parameters store; key 'AdditionalRequestsForSearchInDays' seeded there).
-- @IsVendor still reads the local CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig; all data tables local.
CREATE OR ALTER PROCEDURE [dbo].[CheckIfExistsAdditionalRequestsForImporter]
(
    @ImporterID int,
    @VendorID int = null,
    @CustomerID int = null,
    @CountryID int
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsVendor BIT = CONVERT(BIT, IIF((SELECT TOP 1 1 FROM CRM.CertificateOfOrigins_cf_SupplierDeliveryCountryConfig WHERE ConutryID = @CountryID) = 1, 1, 0));
    DECLARE @DaysForLastDelivery int = (SELECT Value FROM Infrastructure.Parameters WHERE Name = 'AdditionalRequestsForSearchInDays');
    DECLARE @IsExistsRequests BIT;

    IF (@IsVendor = 1)
        BEGIN
            SET @IsExistsRequests = CONVERT(BIT, IIF((SELECT TOP 1 1
                                                      FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
                                                      INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R
                                                          ON R.AuthenticationFileID = F.ID
                                                      WHERE R.ImporterID = @ImporterID
                                                      AND R.VendorId = @VendorID
                                                      AND F.LastDelivery >= DATEADD(DAY, (@DaysForLastDelivery * -1), GETDATE())) = 1, 1, 0));
        END
    ELSE
        BEGIN
            SET @IsExistsRequests = CONVERT(BIT, IIF((SELECT TOP 1 1
                                                      FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails F
                                                      INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationRequest R
                                                          ON R.AuthenticationFileID = F.ID
                                                      WHERE R.ImporterID = @ImporterID
                                                      AND R.CustomerID = @CustomerID
                                                      AND F.LastDelivery >= DATEADD(DAY, (@DaysForLastDelivery * -1), GETDATE())) = 1, 1, 0));
        END

    SELECT @IsExistsRequests;
END
