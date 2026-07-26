-- Source: CRM.usp_CertificateOfOrigins_ExportDocumentAuthenticationRequestSearch (legacy copy, untouched;
--         the .NET const still names it CROSS_, but the live SP dropped that marker).
-- Target: dbo.ExportDocumentAuthenticationRequestSearch - microservice-owned copy.
-- Changes vs legacy:
--   * 3 cross-service JOINs removed (not owned by this service); their name columns return NULL, enriched in BL:
--       - Shared.General_c_Country        (CountryName)             -> ILookupUtil<Country>  (raw CountryId added)
--       - StockPileData.Customers_Customer (ForeignCustomsHouseName) -> ICustomerProxy (keyed on CustomerID)
--       - StockPileData.Customers_Customer (RequestIssuerName)       -> ICustomerProxy (raw ExporterCustomerId added)
--     Local JOINs kept: enum DocumentType (DocumentTypeName), enum ExportAuthenticationRequestStatus
--     (RequestStatusName), OUTER APPLY link-table (ExportDeclarationTitle).
--   * FTS CONTAINS(InvoiceNumbers) -> LIKE (no full-text catalog locally; restores legacy's commented original).
--   * MainDocumentTitle predicate parameterized (was inlined literal).
--   * Removed WITH EXECUTE AS OWNER, READ UNCOMMITTED, and the SSMS Customs_DBA.Script.usp_PrintNvarcharMax block.
--   * @ExportDeclarationID kept as-is (declared but never referenced in the legacy SP - dead param).
CREATE OR ALTER PROCEDURE [dbo].[ExportDocumentAuthenticationRequestSearch]
(
    @CountryID INT,
    @DocumentTypeID INT,
    @RequestID INT,
    @ForeignCustomsHouseID INT,
    @ExportDeclarationID INT,          -- declared but never referenced (dead in legacy; kept faithful)
    @RequestOpenDateFrom DATETIME,
    @RequestOpenDateTo DATETIME,
    @ExportAuthenticationDocumentID INT,
    @InvoiceIDNum NVARCHAR(300),
    @MainDocumentTitle NVARCHAR(255),
    @ExporterCustomerID INT,
    @ExportAuthenticationRequestStatusID INT,
    @CreateUserID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Select NVARCHAR(MAX),
            @From NVARCHAR(MAX),
            @Where NVARCHAR(MAX),
            @OrderBy NVARCHAR(MAX),
            @Filter NVARCHAR(MAX) = N'',
            @TableJoin NVARCHAR(MAX) = N'';

    -- Cross-service name JOINs (Shared.General_c_Country, StockPileData.Customers_Customer x2) removed;
    -- CountryName/ForeignCustomsHouseName/RequestIssuerName return NULL (enriched in BL via lookup/Customer proxy),
    -- raw CountryId + ExporterCustomerId + CustomerID are returned to drive that enrichment.
    SET @Select = N'
    SELECT      EAR.ID AS RequestID,
                CAST(NULL AS NVARCHAR(255)) AS CountryName,
                EAR.CountryID AS CountryId,
                CAST(NULL AS NVARCHAR(255)) AS ForeignCustomsHouseName,
                EAR.CustomerID AS CustomerID,
                DT.Name AS DocumentTypeName,
                LDT.LeadDocumentTitle AS ExportDeclarationTitle,
                EARS.Name AS RequestStatusName,
                CAST(NULL AS NVARCHAR(255)) AS RequestIssuerName,
                EAR.ExporterCustomerID AS ExporterCustomerId,
                EAR.ExportLeadDocumentID,
                EAR.CreateDate AS DocumentIssueDateFrom,
                EAR.AuthenticationRequestArrivalDate AS RequestOpenDateFrom
';

    SET @From = N'
FROM    CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest EAR
        INNER JOIN CRM.CertificateOfOrigins_enum_PrefernceDocumentType DT ON EAR.AuthenticationDocumentTypeID = DT.ID
        INNER JOIN CRM.CertificateOfOrigins_enum_ExportAuthenticationRequestStatus EARS ON EAR.StatusID = EARS.ID
        OUTER APPLY( SELECT TOP 1 coocedarld.LeadDocumentTitle
                     FROM CRM.CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument coocedarld
                     WHERE coocedarld.ExportRequestID = EAR.ID
                     ORDER BY coocedarld.ID ) LDT';

    SET @Where = N'
WHERE   1 = 1';

    SELECT @Filter += CASE WHEN Filter != '' THEN '   AND ('+Filter+') ' ELSE '' END
    FROM (
        SELECT 'EAR.CountryID = @CountryID' Filter
        WHERE @CountryID IS NOT NULL
        UNION ALL
        SELECT 'EAR.AuthenticationDocumentTypeID = @DocumentTypeID'
        WHERE @DocumentTypeID IS NOT NULL
        UNION ALL
        SELECT 'EAR.ID = @RequestID'
        WHERE @RequestID IS NOT NULL
        UNION ALL
        SELECT 'EAR.CustomerID = @ForeignCustomsHouseID'
        WHERE @ForeignCustomsHouseID IS NOT NULL
        UNION ALL
        SELECT 'EAR.CreateDate >= @RequestOpenDateFrom'
        WHERE @RequestOpenDateFrom IS NOT NULL
        UNION ALL
        SELECT 'EAR.CreateDate <= @RequestOpenDateTo'
        WHERE @RequestOpenDateTo IS NOT NULL
        UNION ALL
        SELECT 'EAR.DocumentID = @ExportAuthenticationDocumentID'
        WHERE @ExportAuthenticationDocumentID IS NOT NULL
        UNION ALL
        -- FTS CONTAINS replaced with LIKE (no full-text catalog locally; restores the legacy's commented original)
        SELECT 'EAR.InvoiceNumbers LIKE ''%''+@InvoiceIDNum+''%'''
        WHERE @InvoiceIDNum IS NOT NULL
        UNION ALL
        SELECT 'EAR.MainDocumentTitle LIKE ''%''+@MainDocumentTitle+''%'''
        WHERE @MainDocumentTitle IS NOT NULL
        UNION ALL
        SELECT 'EAR.ExporterCustomerID = @ExporterCustomerID'
        WHERE @ExporterCustomerID IS NOT NULL
        UNION ALL
        SELECT 'EAR.StatusID = @ExportAuthenticationRequestStatusID'
        WHERE @ExportAuthenticationRequestStatusID IS NOT NULL
        UNION ALL
        SELECT N'EAR.CreateUserID = @CreateUserID'
        WHERE @CreateUserID IS NOT NULL
    ) f;

    SET @OrderBy = N'ORDER BY EAR.ID';

    SELECT @Select += @From+@TableJoin+@Where+@Filter+@OrderBy;

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
