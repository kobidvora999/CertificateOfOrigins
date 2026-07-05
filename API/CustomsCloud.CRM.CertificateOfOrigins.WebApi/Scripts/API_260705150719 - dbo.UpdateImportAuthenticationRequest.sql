-- Source: CRM.usp_CertificateOfOrigins_UpdateImportAuthenticationRequest (legacy copy, untouched)
-- Target: dbo.UpdateImportAuthenticationRequest - microservice-owned copy (db-proc STEP 0/6.5)
-- Generated: 2026-07-05 15:07 by db-proc audit
IF SCHEMA_ID('Shared') IS NULL EXEC('CREATE SCHEMA Shared');
GO
IF TYPE_ID('Shared.IntArray') IS NULL
    CREATE TYPE Shared.IntArray AS TABLE ([val] INT);
GO
CREATE OR ALTER PROCEDURE [dbo].[UpdateImportAuthenticationRequest]
       (
       @ImportAuthenticationRequestsID Shared.IntArray READONLY,
       @AuthenticationFileID INT
       )
AS
       BEGIN
              SET NOCOUNT ON;
             
              UPDATE CRM.CertificateOfOrigins_ImportAuthenticationRequest
              SET           AuthenticationFileID = @AuthenticationFileID
              WHERE  CRM.CertificateOfOrigins_ImportAuthenticationRequest.DocumentID IN (SELECT IARI.val
                                     FROM              @ImportAuthenticationRequestsID IARI)
                                                       AND CRM.CertificateOfOrigins_ImportAuthenticationRequest.AuthenticationFileID IS NULL
       END;
GO
