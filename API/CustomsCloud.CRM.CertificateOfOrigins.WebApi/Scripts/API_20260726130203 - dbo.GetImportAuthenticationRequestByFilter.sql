-- Source: CRM.usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter (legacy copy, untouched).
-- Target: dbo.GetImportAuthenticationRequestByFilter - microservice-owned copy.
-- Changes vs legacy:
--   * 5 cross-service JOINs removed (not owned/replicated by this service); their name columns return NULL
--     and are enriched in the BL:
--       - StockPileData.Customers_Customer (ImporterName)  -> ICustomerProxy
--       - StockPileData.Vendors_Vendor      (VendorName)    -> IVendorProxy
--       - Shared.General_c_Country           (IssuingCountryID name) -> ILookupUtil<Country>
--       - Infrastructure.UserMng_OrganizationUnit (OrganizationUnitID name) -> ILookupUtil<OrganizationUnit>
--       - CRP.DealFile_LeadDocument          (LeadDocumentTitle) -> NULL + TODO (raw LeadDocumentID returned)
--     Local JOINs kept: enum_PrefernceDocumentType (PreferenceDocumentTypeID name), ImportAuthenticationFileDetails (AuthenticationFileStatusID).
--   * Shared.ufn_GetMaxRows() -> TOP (200) literal (generic row cap).
--   * Shared.ufn_General_GetDateStart/End -> inline day-boundary math (same as sibling dbo.GetCertificateOfOriginsByFilter).
--   * FTS CONTAINS(R.InvoiceNumber, ...) -> LIKE '%...%' (no full-text catalog in the service DB; restores the
--     legacy's own original commented-out predicate).
--   * Removed WITH EXECUTE AS OWNER, READ UNCOMMITTED hint, EDMX-mapping comment block, and the SSMS-only
--     Customs_DBA.Script.usp_PrintNvarcharMax debug print (cross-DB, not present locally).
CREATE OR ALTER PROCEDURE [dbo].[GetImportAuthenticationRequestByFilter]
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
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Select NVARCHAR(MAX),
            @From NVARCHAR(MAX),
            @Where NVARCHAR(MAX),
            @OrderBy NVARCHAR(MAX),
            @Filter NVARCHAR(MAX) = N'',
            @TableJoin NVARCHAR(MAX) = N'';

    SET @FromRequestDate = CAST(@FromRequestDate AS DATE);
    SET @ToRequestDate   = DATEADD(MILLISECOND, -3, CAST(DATEADD(DAY, 1, CAST(@ToRequestDate AS DATE)) AS DATETIME));

    SET @Select = N'
SELECT  TOP (200)
                R.DocumentID,
                CAST(NULL AS NVARCHAR(255)) IssuingCountryID,
                CAST(NULL AS NVARCHAR(255)) OrganizationUnitID,
                P.Name PreferenceDocumentTypeID,
                R.AuthenticationFileID,
                CAST(NULL AS NVARCHAR(255)) LeadDocumentTitle,
                R.CreateDate,
                CAST(NULL AS NVARCHAR(255)) VendorName,
                R.IssuingCountryID IssuingCountryIDNum,
                R.OrganizationUnitID OrganizationUnitIDNum,
                R.ResponseNameEmail,
                R.LeadDocumentID,
                R.ImporterID CustomerID,
                R.VendorID,
                R.DecisionID,
                CAST(NULL AS NVARCHAR(255)) ImporterName,
                COOIAFD.AuthenticationFileStatusID';

    SET @From = N'
FROM    CRM.CertificateOfOrigins_ImportAuthenticationRequest R
        INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType P ON P.ID = R.PreferenceDocumentTypeID
        LEFT JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails COOIAFD ON R.AuthenticationFileID = COOIAFD.ID
        ';

    SET @Where = N'
WHERE   (R.CreateDate BETWEEN @FromRequestDate And @ToRequestDate)
    ';

    SELECT  @Filter += CASE WHEN Filter != '' THEN '        and ('+Filter+')
    '
                            ELSE '' END
    FROM    (   SELECT  N'R.PreferenceDocumentTypeID = @PrefernceDocumentType' Filter
                WHERE   @PrefernceDocumentType IS NOT NULL
                UNION ALL
                SELECT  N'R.OriginCountryID = @GoodsOrigionCountry'
                WHERE   @GoodsOrigionCountry IS NOT NULL
                UNION ALL
                SELECT  N'R.IssuingCountryID = @IssuingCountry'
                WHERE   @IssuingCountry IS NOT NULL
                UNION ALL
                SELECT  N'R.ImportCountryID = @ImportCountry'
                WHERE   @ImportCountry IS NOT NULL
                UNION ALL
                SELECT  N'R.ImporterID = @ImporterID'
                WHERE   @ImporterID IS NOT NULL
                UNION ALL
                SELECT  N'R.OrganizationUnitID = @CustomsHouseID'
                WHERE   @CustomsHouseID IS NOT NULL
                UNION ALL
                SELECT  N'R.RequestCircumstancesID = @RequestReason'
                WHERE   @RequestReason IS NOT NULL
                UNION ALL
                SELECT  N'R.LeadDocumentID = @LeadDocumentID '
                WHERE   @leadDocumentID IS NOT NULL
                UNION ALL
                SELECT  N'R.VendorID = @VendorID'
                WHERE   @VendorID IS NOT NULL
                UNION ALL
                SELECT  N'R.DecisionID = @DecisionID '
                WHERE   @DecisionID  IS NOT NULL
                UNION ALL
                SELECT  N'R.CustomerID = @CustomerID'
                WHERE   @CustomerID IS NOT NULL
                UNION ALL
                SELECT  N'R.DocumentID = @DocumentID'
                WHERE   @DocumentID IS NOT NULL
                UNION ALL
                SELECT  N'R.InvoiceNumber LIKE ''%''+@InvoiceNumber+''%'''
                WHERE   @InvoiceNumber IS NOT NULL
                UNION ALL
                SELECT  N'R.DocumentNumber LIKE ''%''+@DocumentNumber+''%'''
                WHERE   @DocumentNumber IS NOT NULL
                UNION ALL
                SELECT  N'R.AuthenticationFileID = @AuthenticationFileID'
                WHERE   @AuthenticationFileID IS NOT NULL
                UNION ALL
                SELECT N'R.CreateUserID = @CreateUserID'
                WHERE @CreateUserID IS NOT NULL) t;

    SET @OrderBy = N'
ORDER BY R.CreateDate DESC
    ';

    SET @Select += @From+@TableJoin+@Where+ISNULL(@Filter, N'')+@OrderBy;

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
