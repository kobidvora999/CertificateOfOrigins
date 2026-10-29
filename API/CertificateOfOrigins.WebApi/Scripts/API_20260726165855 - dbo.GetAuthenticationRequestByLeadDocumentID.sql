-- Source: CRM.usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID (legacy copy, untouched).
-- Target: dbo.GetAuthenticationRequestByLeadDocumentID - microservice-owned copy.
-- Takes a Table-Valued Parameter @LeadDocumentIDs Shared.IntArray READONLY (val int) — the TVP type already
-- exists in the service DB, so no CREATE TYPE is needed. The BL binds it via Dapper AsTableValuedParameter.
-- Changes vs legacy:
--   * 3 cross-service JOINs removed (not owned by this service); their name columns return NULL, enriched in BL:
--       - CRP.DealFile_LeadDocument         (LeadDocumentTitle)    -> NULL + TODO (no proxy; raw LeadDocumentID returned)
--       - Shared.General_c_Country          (ImportCountryName)    -> ILookupUtil<Country> (raw ImportCountryID)
--       - Infrastructure.UserMng_OrganizationUnit (OrganizationUnitName) -> ILookupUtil<OrganizationUnit> (raw OrganizationUnitID)
--     Local joins kept: enum PrefernceDocumentType, enum AuthenticationFileStatus, enum Decision, ImportAuthenticationFileDetails.
--   * Removed READ UNCOMMITTED. No dynamic SQL / TOP / UDFs in this SP (plain static SELECT).
CREATE OR ALTER PROCEDURE [dbo].[GetAuthenticationRequestByLeadDocumentID]
(
    @LeadDocumentIDs Shared.IntArray READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cross-service JOINs removed (CRP.DealFile_LeadDocument, Shared.General_c_Country,
    -- Infrastructure.UserMng_OrganizationUnit): LeadDocumentTitle/ImportCountryName/OrganizationUnitName return
    -- NULL and are enriched in the BL (country + org-unit via ILookupUtil; lead-document title left null, no proxy).
    -- Raw IDs (LeadDocumentID, ImportCountryID, OrganizationUnitID) are returned to drive that enrichment.
    SELECT IAR.LeadDocumentID
         ,CAST(NULL AS NVARCHAR(255)) LeadDocumentTitle
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
         ,CAST(NULL AS NVARCHAR(255)) ImportCountryName
         ,IAR.OrganizationUnitID
         ,CAST(NULL AS NVARCHAR(255)) OrganizationUnitName
         ,IAR.CollateralID
         ,CAST((CASE WHEN IAR.CollateralID IS NULL THEN 0 ELSE 1 END) AS BIT) IsCollateralExists
    FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest IAR
    INNER JOIN @LeadDocumentIDs LDs ON IAR.LeadDocumentID = LDs.VAL
    LEFT JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails IAFD ON IAFD.ID = IAR.AuthenticationFileID
    INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType PT ON PT.ID = IAR.PreferenceDocumentTypeID
    LEFT JOIN CRM.CertificateOfOrigins_enum_AuthenticationFileStatus AFS ON AFS.ID = IAFD.AuthenticationFileStatusID
    INNER JOIN CRM.CertificateOfOrigins_enum_Decision D ON D.ID = IAR.DecisionID
END
