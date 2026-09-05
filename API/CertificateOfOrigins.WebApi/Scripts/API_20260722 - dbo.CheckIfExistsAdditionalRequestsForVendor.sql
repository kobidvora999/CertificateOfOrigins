-- Source: CRM.usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForVendor (legacy copy, untouched)
-- Target: dbo.CheckIfExistsAdditionalRequestsForVendor - microservice-owned copy (clean rename;
-- reads the local CRM.CertificateOfOrigins_ImportAuthenticationRequest table, no JOINs/functions to fix).
CREATE OR ALTER PROCEDURE [dbo].[CheckIfExistsAdditionalRequestsForVendor]
(
    @VendorID int
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CONVERT(BIT, IIF((SELECT COUNT(R.DocumentID)
                             FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest R
                             WHERE R.VendorId = @VendorID
                             AND R.CreateDate >= DATEADD(YEAR, (-3), GETDATE())) > 1, 1, 0));
END
