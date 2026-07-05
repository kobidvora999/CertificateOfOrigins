IF SCHEMA_ID('Shared') IS NULL EXEC('CREATE SCHEMA Shared');
GO
IF TYPE_ID('Shared.IntArray') IS NULL
    CREATE TYPE Shared.IntArray AS TABLE ([val] INT);
GO

-- =============================================
-- Author:        Efrat Y.
-- Create date: 23/12/2018 - CR 104519
-- Update date: 26/12/2018
-- Update date: 21/12/2020 ruth_ha CR 154269
-- Description:   usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[GetAuthenticationRequestByLeadDocumentID]
(
      @LeadDocumentIDs Shared.IntArray READONLY
)
AS
BEGIN

      SET NOCOUNT ON;
      SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
      
      SELECT IAR.LeadDocumentID
           ,CAST(NULL AS NVARCHAR(255)) LeadDocumentTitle -- db-proc Pattern F: CRP.DealFile_LeadDocument not replicated
            ,IAR.DocumentID
            ,IAR.AuthenticationFileID
            ,IAR.PreferenceDocumentTypeID
            ,PT.Name PreferenceDocumentTypeName
            ,IAR.CreateDate
            ,IAFD.AuthenticationFileStatusID
            ,AFS.Name AuthenticationFileStatusName
            ,IAR.DecisionID
            ,D.Name DecisionName
            ,IAR.ImportCountryID
            ,CAST(NULL AS NVARCHAR(255)) ImportCountryName -- filled in BL via ILookupUtil<Country>
            ,IAR.OrganizationUnitID
            ,CAST(NULL AS NVARCHAR(255)) OrganizationUnitName -- filled in BL via ILookupUtil<OrganizationUnit>
            ,IAR.CollateralID
            ,CAST((CASE WHEN IAR.CollateralID IS NULL THEN 0 ELSE 1 END) AS BIT) IsCollateralExists
      FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest IAR
      INNER JOIN @LeadDocumentIDs LDs ON IAR.LeadDocumentID = LDs.VAL
      -- INNER JOIN CRP.DealFile_LeadDocument dfld ON LDs.VAL = dfld.ID  (not replicated)
      LEFT JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails IAFD ON IAFD.ID = IAR.AuthenticationFileID
      INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType PT ON PT.ID = IAR.PreferenceDocumentTypeID
      LEFT JOIN CRM.CertificateOfOrigins_enum_AuthenticationFileStatus AFS ON AFS.ID = IAFD.AuthenticationFileStatusID
      INNER JOIN CRM.CertificateOfOrigins_enum_Decision D ON D.ID = IAR.DecisionID
      -- INNER JOIN Shared.General_c_Country C ON C.ID = IAR.ImportCountryID  (not replicated)
      -- INNER JOIN Infrastructure.UserMng_OrganizationUnit OU ON OU.ID = IAR.OrganizationUnitID  (not replicated)
      
END
GO
