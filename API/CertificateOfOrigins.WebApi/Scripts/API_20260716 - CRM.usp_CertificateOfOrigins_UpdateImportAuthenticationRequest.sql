SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_UpdateImportAuthenticationRequest]
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