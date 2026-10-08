-- Amends dbo.GetCertificateOfOriginsByFilter (previous version: "API_20260721 - dbo.GetCertificateOfOriginsByFilter.sql", kept as the historical record -
-- DbUp keys on the file name, so an applied script edited in place would never re-run).
--
-- The "to" date bound follows the legacy Shared.ufn_General_GetDateEnd: the date + 86399 seconds = 23:59:59.000
-- (confirmed from the function body in the legacy DB, 2026-10-08). The previous version used 23:59:59.997, which also took
-- in rows of the last second of the day that legacy left out (parity finding A-25). The "from" bound (midnight, legacy
-- ufn_General_GetDateStart) was already equal.
CREATE OR ALTER PROCEDURE [dbo].[GetCertificateOfOriginsByFilter]
    @certificateNumber NVARCHAR(35) ,
    @certificateOfOriginStatusID INT ,
    @certificateOfOriginTypeID INT ,
    @customsAgentID INT ,
    @customsHouseID INT ,
    @destinationCountry INT ,
    @exportDeclarationID INT ,
    @exportDeclarationNum NVARCHAR(35),
    @exporterCustomerID INT ,
    @fromIssuingDate DATETIME ,
    @toIssuingDate DATETIME ,
    @fromRequestDate DATETIME ,
    @toRequestDate DATETIME ,
    @requestReasonID INT,
      @versionNumber INT,
      @isLastVersion BIT
AS
    BEGIN
        SET NOCOUNT ON;
        DECLARE @Select NVARCHAR(MAX) ,
            @From NVARCHAR(MAX) ,
            @Where NVARCHAR(MAX) ,
            @OrderBy NVARCHAR(MAX) ,
            @Filter NVARCHAR(MAX) = '' ,
            @TableJoin NVARCHAR(MAX) = ''

        SET @Select = '
-- TOP (200) = Shared.ufn_GetMaxRows() of the legacy procedure; production returns 200 (confirmed 2026-10-04).
SELECT      TOP (200)
                        F.ID,
                F.CertificateNumber,
                S.Name,
                F.CreateCustomerID CustomesAgentID,
                CAST(NULL AS NVARCHAR(255)) CustomesAgentTitle,
                CAST(NULL AS NVARCHAR(50)) CustomesAgentExternalIdNum,
                F.CustomerID ExporterID,
                CAST(NULL AS NVARCHAR(255)) ExporterTitle,
                CAST(NULL AS NVARCHAR(50)) ExporterExternalIdNum,
                F.ExportDeclarationNumber,
                        F.VersionNumber,
                        F.OrganizationUnitID,
                        F.RequestReasonCode,
                        F.IssuingDate,
                        F.LeadDocumentID
            '
        SET @From = '
FROM    CRM.CertificateOfOrigins_CertificateOfOrigin F
        INNER JOIN CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode S ON f.CertificateOfOriginStatusID = S.ID
    '

        SET @Where = '
WHERE (F.State = 1)
    '
            SET @certificateNumber = '%' + @certificateNumber + '%'

        SELECT  @Filter += CASE WHEN Filter != ''
                                THEN '          and (' + Filter + ')
    '                           ELSE ''
                           END
        FROM    ( SELECT    N'F.CertificateNumber LIKE @certificateNumber' Filter
                  WHERE     @certificateNumber IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CertificateOfOriginStatusID = @certificateOfOriginStatusID'
                  WHERE     @certificateOfOriginStatusID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.TypeID = @certificateOfOriginTypeID'
                  WHERE     @certificateOfOriginTypeID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CustomerID = @exporterCustomerID'
                  WHERE     @exporterCustomerID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateCustomerID = @customsAgentID'
                  WHERE     @customsAgentID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.OrganizationUnitID = @customsHouseID'
                  WHERE     @customsHouseID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.DestinationCountry = @destinationCountry'
                  WHERE     @destinationCountry IS NOT NULL
                  UNION ALL
                  SELECT    N'F.IssuingDate >= CAST(@fromIssuingDate AS DATE)'
                  WHERE     @fromIssuingDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.IssuingDate <= DATEADD(SECOND, 86399, CAST(CAST(@toIssuingDate AS DATE) AS DATETIME))'
                  WHERE     @toIssuingDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate >= CAST(@fromRequestDate AS DATE)'
                  WHERE     @fromRequestDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate <= DATEADD(SECOND, 86399, CAST(CAST(@toRequestDate AS DATE) AS DATETIME))'
                  WHERE     @toRequestDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.RequestReasonCode = @requestReasonID'
                  WHERE     @requestReasonID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.LeadDocumentID = @exportDeclarationID'
                  WHERE     @exportDeclarationID IS NOT NULL
                  UNION ALL
                  SELECT    N'F.ExportDeclarationNumber = @exportDeclarationNum'
                  WHERE     @exportDeclarationNum IS NOT NULL
                          UNION ALL
                          SELECT    N'F.VersionNumber = @versionNumber'
                  WHERE     @versionNumber IS NOT NULL
                  UNION ALL
                  SELECT    N'F.IsLastVersion = @isLastVersion'
                  WHERE     @isLastVersion IS NOT NULL
                ) t

        SET @OrderBy = '
      ORDER BY F.CreateDate DESC
    '

        SET @Select += @From + @TableJoin + @Where + @Filter + @OrderBy

        EXEC sys.sp_executesql @Select, N'    @certificateNumber NVARCHAR(35) ,
    @certificateOfOriginStatusID INT ,
    @certificateOfOriginTypeID INT ,
    @customsAgentID INT ,
    @customsHouseID INT ,
    @destinationCountry INT ,
    @exportDeclarationID INT ,
    @exportDeclarationNum NVARCHAR(35),
    @exporterCustomerID INT ,
    @fromIssuingDate DATETIME ,
    @toIssuingDate DATETIME ,
    @fromRequestDate DATETIME ,
    @toRequestDate DATETIME ,
    @requestReasonID INT,
      @versionNumber INT ,
    @isLastVersion BIT',
      @certificateNumber = @certificateNumber ,
    @certificateOfOriginStatusID = @certificateOfOriginStatusID ,
    @certificateOfOriginTypeID = @certificateOfOriginTypeID ,
    @customsAgentID = @customsAgentID ,
    @customsHouseID = @customsHouseID ,
    @destinationCountry = @destinationCountry ,
    @exportDeclarationID = @exportDeclarationID ,
    @exportDeclarationNum = @exportDeclarationNum ,
    @exporterCustomerID = @exporterCustomerID ,
    @fromIssuingDate = @fromIssuingDate ,
    @toIssuingDate = @toIssuingDate ,
    @fromRequestDate = @fromRequestDate ,
    @toRequestDate = @toRequestDate ,
    @requestReasonID = @requestReasonID ,
      @versionNumber = @versionNumber ,
      @isLastVersion = @isLastVersion

END
