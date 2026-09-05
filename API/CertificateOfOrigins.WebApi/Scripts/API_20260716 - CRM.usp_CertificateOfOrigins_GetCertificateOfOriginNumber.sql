SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author:		Shiran David
-- Create date: 20.3.14
-- Description:	Get certificate number
-- Update date: 25.6.15 - (aviram_be) added sequencer as certificate number enumerator
-- =============================================
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginNumber]
 
AS
BEGIN
 
	SET NOCOUNT ON;
	SELECT  NEXT VALUE FOR CRM.sq_CertificateOfOrigins_CertificateOfOrigin AS Column1
END