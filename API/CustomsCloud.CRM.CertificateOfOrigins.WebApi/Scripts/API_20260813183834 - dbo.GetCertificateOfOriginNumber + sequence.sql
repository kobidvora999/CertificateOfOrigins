USE [CertificateOfOrigins];
GO

-- GetPC_MSG2280_2281 create branch: the certificate-number generator (legacy
-- CertificateOfOriginsBL.GetCertificateNumber → usp_CertificateOfOrigins_GetCertificateOfOriginNumber, which returns
-- NEXT VALUE FOR CRM.sq_CertificateOfOrigins_CertificateOfOrigin; the BL prefixes "IL" + 10-digit format).
-- The monolith owns the CRM.usp_* copy; this microservice calls its own dbo.GetCertificateOfOriginNumber copy.
--
-- The sequence object was missing from the local DB (the monolith seeds it elsewhere). Create it if absent, starting
-- past the highest existing IL-numbered certificate (IL0000116895) so generated numbers do not collide.
IF NOT EXISTS (SELECT 1 FROM sys.sequences seq JOIN sys.schemas s ON seq.schema_id = s.schema_id
               WHERE s.name = 'CRM' AND seq.name = 'sq_CertificateOfOrigins_CertificateOfOrigin')
BEGIN
    CREATE SEQUENCE CRM.sq_CertificateOfOrigins_CertificateOfOrigin
        AS INT
        START WITH 116896
        INCREMENT BY 1;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetCertificateOfOriginNumber]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT NEXT VALUE FOR CRM.sq_CertificateOfOrigins_CertificateOfOrigin AS Column1;
END
GO
