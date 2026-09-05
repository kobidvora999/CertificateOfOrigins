SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
/*=============================================
-- Author: UNKNOWN
-- Create date: UNKNOWN
-- Update date: 15/04/2019 Efrat_Y - CR 126549 add R.DecisionID
-- Description: usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter
-- update: 11/06/2019 - 126569 - Ori - Fixed CustomerID instead of VendorID
-- update: 13/06/2019 - 126569 - Ori - Added ImporterName
-- update: 25/06/2019 - 135681 - Assaf Perl
-- update: 25/06/2019 - 136427 - Ori_Ko Added AuthenticationFileStatusID
-- update: 04/11/2019 - 138077 - tamar_ho Add parameters
-- update: 27/11/2019 - 139580 - tamar_ho LEFT JOIN to Vendor
-- update: 22/10/2020 - 153488 - Dean_Ha Added CreateUserID
--			05/11/2020 Maxim  - Search by @InvoiceIDNumFTS
-- =============================================*/
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter]
	----DECLARE
	@PrefernceDocumentType INT,
	@GoodsOrigionCountry INT,
	@IssuingCountry INT,
	@ImportCountry INT,
	@FromRequestDate DATETIME,
	@ToRequestDate DATETIME,
	@CustomsHouseID INT,
	@RequestReason INT,
	@leadDocumentID INT,
	@ImporterID INT,
	@VendorID INT,
	@DecisionID INT,
	@CustomerID INT,
	@DocumentID INT,
	@InvoiceNumber NVARCHAR(255),
	@DocumentNumber NVARCHAR(255),
	@AuthenticationFileID INT,
	@CreateUserID INT
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
 
	DECLARE @Select NVARCHAR(MAX),
			@From NVARCHAR(4000),
			@Where NVARCHAR(4000),
			@OrderBy NVARCHAR(4000),
			@Filter NVARCHAR(4000) = N'',
			@TableJoin NVARCHAR(4000) = N'';
 
	SET @FromRequestDate = Shared.ufn_General_GetDateStart(@FromRequestDate);
	SET @ToRequestDate = Shared.ufn_General_GetDateEnd(@ToRequestDate);
	DECLARE @InvoiceIDNumFTS NVARCHAR(320)  = '''' + REPLACE(LTRIM(RTRIM(@InvoiceNumber)), ',', ' OR ') + ''''
 
	--EDMX MAPPING -- do not remove
	--SET FMTONLY OFF
 
	--SELECT
	--	CAST(1 AS INT) DocumentID
	--   ,CAST('' AS NVARCHAR(255)) IssuingCountryID
	--   ,CAST('' AS NVARCHAR(255)) OrganizationUnitID
	--   ,CAST('' AS NVARCHAR(255)) PreferenceDocumentTypeID
	--   ,CAST(NULL AS INT) AuthenticationFileID
	--   ,CAST('' AS NVARCHAR(14)) LeadDocumentTitle
	--   ,GETDATE() CreateDate
	--   ,CAST('' AS NVARCHAR(255)) VendorName
	--   ,CAST(1 AS INT) IssuingCountryIDNum
	--   ,CAST(1 AS INT) OrganizationUnitIDNum
	--   ,CAST('' AS NVARCHAR(14)) ResponseNameEmail
	--   ,CAST(1 AS INT) LeadDocumentID
	--   ,CAST(1 AS INT) CustomerID
	--   ,CAST(1 AS INT) VendorID
	--   ,CAST(1 AS INT) DecisionID
	--   ,CAST('' AS NVARCHAR(255)) ImporterName
	--   ,CAST(1 AS INT) AuthenticationFileStatusID
 
	--RETURN
 
 
	SET @Select = N' 	
SELECT 	TOP (shared.ufn_GetMaxRows())
				R.DocumentID,
				C.Name IssuingCountryID,
				O.Title OrganizationUnitID,
				P.Name PreferenceDocumentTypeID,
				R.AuthenticationFileID,
				DFLD.Title LeadDocumentTitle, 
				R.CreateDate,
				vv.Title VendorName,
				R.IssuingCountryID IssuingCountryIDNum,
				R.OrganizationUnitID OrganizationUnitIDNum,
				R.ResponseNameEmail,
				R.LeadDocumentID,
				R.ImporterID CustomerID,
				R.VendorID,
				R.DecisionID,
				Customer.Title ImporterName,
				COOIAFD.AuthenticationFileStatusID';
 
	SET @From = N' 
FROM    CRM.CertificateOfOrigins_ImportAuthenticationRequest R
       	INNER JOIN CRP.DealFile_LeadDocument dfld ON dfld.ID = R.LeadDocumentID
		INNER JOIN Shared.General_c_Country C ON C.ID = R.IssuingCountryID
		INNER JOIN Infrastructure.UserMng_OrganizationUnit O ON O.ID = R.OrganizationUnitID
		INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType P ON P.ID = R.PreferenceDocumentTypeID
		LEFT JOIN StockPileData.Vendors_Vendor vv ON vv.ID = R.VendorID
		LEFT JOIN StockPileData.Customers_Customer Customer ON Customer.ID = R.ImporterID
		LEFT JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD ON R.AuthenticationFileID = COOIAFD.ID
		';
 
	SET @Where = N'	
WHERE	(R.CreateDate BETWEEN @FromRequestDate And @ToRequestDate)		
    ';
 
	SELECT	@Filter += CASE WHEN Filter ! = '' THEN '		and ('+Filter+')
    '
							ELSE '' END
	FROM	(	SELECT	N'R.PreferenceDocumentTypeID = @PrefernceDocumentType' Filter
				WHERE	@PrefernceDocumentType IS NOT NULL
				UNION ALL
				SELECT	N'R.OriginCountryID = @GoodsOrigionCountry'
				WHERE	@GoodsOrigionCountry IS NOT NULL
				UNION ALL
				SELECT	N'R.IssuingCountryID = @IssuingCountry'
				WHERE	@IssuingCountry IS NOT NULL
				UNION ALL
				SELECT	N'R.ImportCountryID = @ImportCountry'
				WHERE	@ImportCountry IS NOT NULL
				UNION ALL
				SELECT	N'R.ImporterID = @ImporterID'
				WHERE	@ImporterID IS NOT NULL
				UNION ALL
				SELECT	N'R.OrganizationUnitID = @CustomsHouseID'
				WHERE	@CustomsHouseID IS NOT NULL
				UNION ALL
				SELECT	N'R.RequestCircumstancesID = @RequestReason'
				WHERE	@RequestReason IS NOT NULL
				UNION ALL
				SELECT	N'R.LeadDocumentID = @LeadDocumentID '
				WHERE	@leadDocumentID IS NOT NULL				
				UNION ALL
				SELECT	N'R.VendorID = @VendorID'
				WHERE	@VendorID IS NOT NULL
				UNION ALL
				SELECT	N'R.DecisionID = @DecisionID '
				WHERE	@DecisionID  IS NOT NULL
				UNION ALL
				SELECT	N'R.CustomerID = @CustomerID'
				WHERE	@CustomerID IS NOT NULL
				UNION ALL
				SELECT	N'R.DocumentID = @DocumentID'
				WHERE	@DocumentID IS NOT NULL
				UNION ALL
				--SELECT	N'R.InvoiceNumber LIKE ''%''+@InvoiceNumber+''%'''
				--WHERE	@InvoiceNumber IS NOT NULL
				SELECT	'CONTAINS ( R.InvoiceNumber ,' +  @InvoiceIDNumFTS +')'
				WHERE	@InvoiceIDNumFTS IS NOT NULL
				UNION ALL
				SELECT	N'R.DocumentNumber LIKE ''%''+@DocumentNumber+''%'''
				WHERE	@DocumentNumber IS NOT NULL
				UNION ALL
				SELECT	N'R.AuthenticationFileID = @AuthenticationFileID'
				WHERE	@AuthenticationFileID IS NOT NULL
				UNION ALL
				SELECT N'R.CreateUserID = @CreateUserID'
				WHERE @CreateUserID IS NOT NULL) t;
 
	SET @OrderBy = N'    
ORDER BY R.CreateDate DESC
    ';
 
	SET @Select += @From+@TableJoin+@Where+ISNULL(@Filter, '')+@OrderBy;
 
	IF APP_NAME() = 'Microsoft SQL Server Management Studio - Query'
	BEGIN
		EXEC [Customs_DBA].[Script].[usp_PrintNvarcharMax] @Select ;
	END
 
 
	/**/
	EXEC sys.sp_executesql @Select,
						   N'@PrefernceDocumentType INT,
							 @GoodsOrigionCountry INT,
							 @IssuingCountry INT,
							 @ImportCountry INT,
							 @FromRequestDate DATETIME,
							 @ToRequestDate DATETIME,
							 @CustomsHouseID INT,
							 @RequestReason INT,
							 @LeadDocumentID INT,
							 @ImporterID INT,
							 @VendorID INT,
							 @DecisionID INT,
							 @CustomerID INT,
							 @DocumentID INT,
							 @InvoiceNumber NVARCHAR(255),
							 @DocumentNumber NVARCHAR(255),
							 @AuthenticationFileID INT,
							 @CreateUserID INT',
						     @PrefernceDocumentType = @PrefernceDocumentType,
						     @GoodsOrigionCountry = @GoodsOrigionCountry,
						     @IssuingCountry = @IssuingCountry,
						     @ImportCountry = @ImportCountry,
						     @FromRequestDate = @FromRequestDate,
						     @ToRequestDate = @ToRequestDate,
						     @CustomsHouseID = @CustomsHouseID,
						     @RequestReason = @RequestReason,
						     @leadDocumentID = @leadDocumentID,
						     @ImporterID = @ImporterID,
						     @VendorID = @VendorID,
							 @DecisionID = @DecisionID ,
							 @CustomerID = @CustomerID,
							 @DocumentID = @DocumentID,
							 @InvoiceNumber = @InvoiceNumber,
							 @DocumentNumber = @DocumentNumber,
							 @AuthenticationFileID = @AuthenticationFileID,
							 @CreateUserID = @CreateUserID;
 
END;