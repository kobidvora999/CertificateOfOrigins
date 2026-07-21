SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
--=============================================
--Author: Tamar Hoorvitz      
--Create date: 14/11/2019
--Description Check If Exists Additional Requests For Vendor.
-- 20/01 - DeanH - BUG -148346
--=============================================
CREATE OR ALTER   PROCEDURE [CRM].[usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForVendor]
(
	@VendorID int
)
AS 
BEGIN	
    SET NOCOUNT ON;
 
	--DECLARE @VendorID int=3065760
	SELECT CONVERT(BIT, IIF((SELECT	COUNT(R.DocumentID)
							 FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest R 
							 WHERE R.VendorId = @VendorID
							 AND R.CreateDate > = DATEADD(YEAR,(-3),GETDATE())) > 1, 1, 0))
END
 