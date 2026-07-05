-- Source: CRM.usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter (legacy copy, untouched)
-- Target: dbo.GetImportAuthenticationRequestByFilter - microservice-owned copy (db-proc STEP 0/6.5)
-- Generated: 2026-07-05 15:07 by db-proc audit
CREATE OR ALTER PROCEDURE [dbo].[GetImportAuthenticationRequestByFilter]
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

      SET @FromRequestDate = CAST(@FromRequestDate AS DATE);
      SET @ToRequestDate = DATEADD(MILLISECOND, -3, CAST(DATEADD(DAY, 1, CAST(@ToRequestDate AS DATE)) AS DATETIME));
      DECLARE @InvoiceIDNumFTS NVARCHAR(4000)  = '''' + REPLACE(LTRIM(RTRIM(@InvoiceNumber)), ',', ' OR ') + ''''

      --EDMX MAPPING -- do not remove
      --SET FMTONLY OFF

      --SELECT
      --    CAST(1 AS INT) DocumentID
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
SELECT      TOP (200)
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
WHERE (R.CreateDate BETWEEN @FromRequestDate And @ToRequestDate)        
    ';

      SELECT      @Filter += CASE WHEN Filter ! = '' THEN '       and ('+Filter+')
    '
                                          ELSE '' END
      FROM  (     SELECT      N'R.PreferenceDocumentTypeID = @PrefernceDocumentType' Filter
                        WHERE @PrefernceDocumentType IS NOT NULL
                        UNION ALL
                        SELECT      N'R.OriginCountryID = @GoodsOrigionCountry'
                        WHERE @GoodsOrigionCountry IS NOT NULL
                        UNION ALL
                        SELECT      N'R.IssuingCountryID = @IssuingCountry'
                        WHERE @IssuingCountry IS NOT NULL
                        UNION ALL
                        SELECT      N'R.ImportCountryID = @ImportCountry'
                        WHERE @ImportCountry IS NOT NULL
                        UNION ALL
                        SELECT      N'R.ImporterID = @ImporterID'
                        WHERE @ImporterID IS NOT NULL
                        UNION ALL
                        SELECT      N'R.OrganizationUnitID = @CustomsHouseID'
                        WHERE @CustomsHouseID IS NOT NULL
                        UNION ALL
                        SELECT      N'R.RequestCircumstancesID = @RequestReason'
                        WHERE @RequestReason IS NOT NULL
                        UNION ALL
                        SELECT      N'R.LeadDocumentID = @LeadDocumentID '
                        WHERE @leadDocumentID IS NOT NULL                     
                        UNION ALL
                        SELECT      N'R.VendorID = @VendorID'
                        WHERE @VendorID IS NOT NULL
                        UNION ALL
                        SELECT      N'R.DecisionID = @DecisionID '
                        WHERE @DecisionID  IS NOT NULL
                        UNION ALL
                        SELECT      N'R.CustomerID = @CustomerID'
                        WHERE @CustomerID IS NOT NULL
                        UNION ALL
                        SELECT      N'R.DocumentID = @DocumentID'
                        WHERE @DocumentID IS NOT NULL
                        UNION ALL
                        --SELECT    N'R.InvoiceNumber LIKE ''%''+@InvoiceNumber+''%'''
                        --WHERE     @InvoiceNumber IS NOT NULL
                        SELECT      N'R.InvoiceNumber LIKE ''%''+@InvoiceNumber+''%''' -- FTS CONTAINS replaced: no local fulltext catalog (legacy pre-FTS variant)
                        WHERE @InvoiceIDNumFTS IS NOT NULL
                        UNION ALL
                        SELECT      N'R.DocumentNumber LIKE ''%''+@DocumentNumber+''%'''
                        WHERE @DocumentNumber IS NOT NULL
                        UNION ALL
                        SELECT      N'R.AuthenticationFileID = @AuthenticationFileID'
                        WHERE @AuthenticationFileID IS NOT NULL
                        UNION ALL
                        SELECT N'R.CreateUserID = @CreateUserID'
                        WHERE @CreateUserID IS NOT NULL) t;

      SET @OrderBy = N'    
ORDER BY R.CreateDate DESC
    ';

      SET @Select += @From+@TableJoin+@Where+ISNULL(@Filter, '')+@OrderBy;



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

GO
