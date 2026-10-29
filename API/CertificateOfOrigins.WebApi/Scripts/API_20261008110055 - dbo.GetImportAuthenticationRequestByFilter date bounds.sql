-- Amends dbo.GetImportAuthenticationRequestByFilter (previous version: "API_20260907180000 - dbo.GetImportAuthenticationRequestByFilter multi-invoice.sql", kept as the historical record -
-- DbUp keys on the file name, so an applied script edited in place would never re-run).
--
-- The "to" date bound follows the legacy Shared.ufn_General_GetDateEnd: the date + 86399 seconds = 23:59:59.000
-- (confirmed from the function body in the legacy DB, 2026-10-08). The previous version used 23:59:59.997, which also took
-- in rows of the last second of the day that legacy left out (parity finding A-25). The "from" bound (midnight, legacy
-- ufn_General_GetDateStart) was already equal.
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
    SET @ToRequestDate   = DATEADD(SECOND, 86399, CAST(CAST(@ToRequestDate AS DATE) AS DATETIME));

    SET @Select = N'
-- TOP (200) = Shared.ufn_GetMaxRows() of the legacy procedure; production returns 200 (confirmed 2026-10-04).
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
                SELECT  N'EXISTS (SELECT 1 FROM STRING_SPLIT(@InvoiceNumber, '','') s
                                  WHERE LTRIM(RTRIM(s.value)) <> ''''
                                    AND R.InvoiceNumber LIKE ''%''+LTRIM(RTRIM(s.value))+''%'')'
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
