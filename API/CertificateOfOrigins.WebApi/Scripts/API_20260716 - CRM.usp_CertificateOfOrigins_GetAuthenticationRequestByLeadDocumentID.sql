SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
-- =============================================
-- Author:		Efrat Y.
-- Create date: 23/12/2018 - CR 104519
-- Update date: 26/12/2018
-- Update date: 21/12/2020 ruth_ha CR 154269
-- Description:	usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID
-- =============================================
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetAuthenticationRequestByLeadDocumentID]
(
	@LeadDocumentIDs Shared.IntArray READONLY
)
AS 
BEGIN
 
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	SELECT IAR.LeadDocumentID
	     ,dfld.Title LeadDocumentTitle
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
		,C.Name ImportCountryName
		,IAR.OrganizationUnitID
		,OU.Title OrganizationUnitName
		,IAR.CollateralID
		,CAST((CASE WHEN IAR.CollateralID IS NULL THEN 0 ELSE 1 END) AS BIT) IsCollateralExists
	FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest IAR
	INNER JOIN @LeadDocumentIDs LDs ON IAR.LeadDocumentID = LDs.VAL
	INNER JOIN CRP.DealFile_LeadDocument dfld ON LDs.VAL = dfld.ID 
	LEFT JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails IAFD ON IAFD.ID = IAR.AuthenticationFileID
	INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType PT ON PT.ID = IAR.PreferenceDocumentTypeID
	LEFT JOIN CRM.CertificateOfOrigins_enum_AuthenticationFileStatus AFS ON AFS.ID = IAFD.AuthenticationFileStatusID
	INNER JOIN CRM.CertificateOfOrigins_enum_Decision D ON D.ID = IAR.DecisionID
	INNER JOIN Shared.General_c_Country C ON C.ID = IAR.ImportCountryID
	INNER JOIN Infrastructure.UserMng_OrganizationUnit OU ON OU.ID = IAR.OrganizationUnitID
END