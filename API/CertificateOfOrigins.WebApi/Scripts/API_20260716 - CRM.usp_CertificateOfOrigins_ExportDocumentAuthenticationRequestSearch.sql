SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
/* =============================================
-- Author:		Novitsky Vitaly
-- Create date: 19/12/2016
-- Description: Creating usp_CertificateOfOrigins_CROSS_ExportDocumentAuthenticationRequestSearch - IT 105720
-- Update:		03/01/2017 vitaly_no - IT 106763 - changing data structure
-- Update:		05/02/2017 idan_sh - IT 107516 - adding customerID and adding ORDER BY ID
-- Update:		02/07/2019 Assaf Perl - Bug 135947, 135948
-- Update:		08/07/2019 Ori_Ko - Bug 135956, 135944
-- Update:		06/10/2019 Assaf Perl - CR 135284
-- Update:		29/10/2019 tamar_ho - CR 135284 - ExportAuthenticationRequestStatus
-- update:		22/10/2020 - 153488 - Dean_Ha Added CreateUserID
-- Update:      28/01/2020 Dean_Ha - BUG 148534 - GetLeadDocument
--				05/11/2020 Maxim  - Search by @InvoiceIDNumFTS
-- =============================================*/
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_ExportDocumentAuthenticationRequestSearch]
(
--declare
	@CountryID INT,
	@DocumentTypeID INT,
	@RequestID INT,
	@ForeignCustomsHouseID INT,
	@ExportDeclarationID INT,	
	@RequestOpenDateFrom DATETIME,
	@RequestOpenDateTo DATETIME,	
	@ExportAuthenticationDocumentID INT,
	@InvoiceIDNum NVARCHAR(300),
	@MainDocumentTitle NVARCHAR(255),
	@ExporterCustomerID INT,
	@ExportAuthenticationRequestStatusID INT,
	@CreateUserID INT
) WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
 
	DECLARE @InvoiceIDNumFTS NVARCHAR(320)  = '''' + REPLACE(LTRIM(RTRIM(@InvoiceIDNum)), ',', ' OR ') + ''''
	DECLARE @Select NVARCHAR(MAX),
			@From NVARCHAR(4000),
			@Where NVARCHAR(4000),
			@OrderBy NVARCHAR(4000),
			@Filter NVARCHAR(4000) = N'',
			@TableJoin NVARCHAR(4000) = N'';
 
	SET @Select = N'	
	SELECT		EAR.ID AS RequestID,
				C.Name AS CountryName,
				CC.Title AS ForeignCustomsHouseName,
				CC.ID AS CustomerID,
				DT.Name AS DocumentTypeName,
				LDT.LeadDocumentTitle AS ExportDeclarationTitle,
				EARS.Name AS RequestStatusName,
				CC1.Title AS RequestIssuerName,
				EAR.ExportLeadDocumentID,
				EAR.CreateDate AS DocumentIssueDateFrom,
				EAR.AuthenticationRequestArrivalDate AS RequestOpenDateFrom
';
 
	SET @From = N'
FROM	CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest EAR
		INNER JOIN Shared.General_c_Country C ON EAR.CountryID = C.ID
		INNER JOIN StockPileData.Customers_Customer CC ON EAR.CustomerID = CC.ID
		INNER JOIN StockPileData.Customers_Customer CC1 ON EAR.ExporterCustomerID = CC1.ID
		INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType DT ON EAR.AuthenticationDocumentTypeID = DT.ID
		INNER JOIN CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus EARS ON EAR.StatusID = EARS.ID
		OUTER APPLY(	SELECT TOP 1 coocedarld.LeadDocumentTitle
						FROM CRM.CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument coocedarld
						WHERE coocedarld.ExportRequestID = EAR.ID
						ORDER BY coocedarld.ID ) LDT'
	SET @Where = N'
WHERE	1 = 1'
 
	SELECT	@Filter += CASE WHEN Filter ! = '' THEN '   AND ('+Filter+') '
						   ELSE '' END
	FROM		
	(
		SELECT	'EAR.CountryID = @CountryID' Filter
		WHERE	@CountryID IS NOT NULL
		UNION ALL
		SELECT	'EAR.AuthenticationDocumentTypeID = @DocumentTypeID' Filter
		WHERE	@DocumentTypeID IS NOT NULL
		UNION ALL
		SELECT	'EAR.ID = @RequestID' Filter
		WHERE	@RequestID IS NOT NULL
		UNION ALL
		SELECT	'EAR.CustomerID = @ForeignCustomsHouseID' Filter
		WHERE	@ForeignCustomsHouseID IS NOT NULL
		UNION ALL		
		SELECT	'EAR.CreateDate >= @RequestOpenDateFrom' Filter
		WHERE	@RequestOpenDateFrom IS NOT NULL
		UNION ALL
		SELECT	'EAR.CreateDate <= @RequestOpenDateTo' Filter
		WHERE	@RequestOpenDateTo IS NOT NULL
		UNION ALL
		SELECT	'EAR.DocumentID = @ExportAuthenticationDocumentID'
		WHERE	@ExportAuthenticationDocumentID IS NOT NULL
		UNION ALL
		--SELECT	'EAR.InvoiceNumbers LIKE ' + '''%' + @InvoiceIDNum + '%''' 
		--WHERE	@InvoiceIDNum IS NOT NULL
		SELECT	'CONTAINS ( EAR.InvoiceNumbers,' +  @InvoiceIDNumFTS +')'
		WHERE	@InvoiceIDNumFTS IS NOT NULL
		UNION ALL
		SELECT	'EAR.MainDocumentTitle LIKE ' + '''%' + @MainDocumentTitle + '%''' 
		WHERE	@MainDocumentTitle IS NOT NULL		
		UNION ALL
		SELECT	'EAR.ExporterCustomerID = @ExporterCustomerID'
		WHERE	@ExporterCustomerID IS NOT NULL
		UNION ALL
		SELECT	'EAR.StatusID = @ExportAuthenticationRequestStatusID'
		WHERE	@ExportAuthenticationRequestStatusID IS NOT NULL
		UNION ALL
		SELECT N'EAR.CreateUserID = @CreateUserID'
		WHERE @CreateUserID IS NOT NULL
	) f;
 
	SET @OrderBy = N'ORDER BY EAR.ID'
 
	SELECT	@Select += @From+@TableJoin+@Where+@Filter+@OrderBy;
 
	IF APP_NAME() = 'Microsoft SQL Server Management Studio - Query'
	BEGIN
		EXEC [Customs_DBA].[Script].[usp_PrintNvarcharMax] @Select;
	END
 
	EXEC sp_executesql @Select, 
	N'@CountryID INT,
	@DocumentTypeID INT,
	@RequestID INT,
	@ForeignCustomsHouseID INT,
	@ExportDeclarationID INT,
	@RequestOpenDateFrom DATETIME,
	@RequestOpenDateTo DATETIME,
	@ExportAuthenticationDocumentID INT,
	@InvoiceIDNum NVARCHAR(300),
	@MainDocumentTitle NVARCHAR(255),
	@ExporterCustomerID INT,
	@ExportAuthenticationRequestStatusID INT,
	@CreateUserID INT',
	@CountryID = @CountryID,
	@DocumentTypeID = @DocumentTypeID,
	@RequestID = @RequestID,
	@ForeignCustomsHouseID = @ForeignCustomsHouseID,
	@ExportDeclarationID = @ExportDeclarationID,
	@RequestOpenDateFrom = @RequestOpenDateFrom,
	@RequestOpenDateTo = @RequestOpenDateTo,
	@ExportAuthenticationDocumentID = @ExportAuthenticationDocumentID,
	@InvoiceIDNum = @InvoiceIDNum,
	@MainDocumentTitle = @MainDocumentTitle,	
	@ExporterCustomerID = @ExporterCustomerID,	
	@ExportAuthenticationRequestStatusID = @ExportAuthenticationRequestStatusID,
	@CreateUserID = @CreateUserID;
 
END;
 