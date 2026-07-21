-- Source: CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter (legacy copy, untouched)
-- Target: dbo.GetCertificateOfOriginsByFilter - microservice-owned copy.
-- The two Shared.rStockPileData_Customers_Customer INNER JOINs are removed (that table is not owned/replicated
-- by this service); raw F.CreateCustomerID (CustomesAgentID) and F.CustomerID (ExporterID) are returned and the
-- agent/exporter titles are enriched in the BL via the Customers proxy (see FillCustomersInformation).
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
                  SELECT    N'F.IssuingDate <= DATEADD(MILLISECOND, -3, CAST(DATEADD(DAY, 1, CAST(@toIssuingDate AS DATE)) AS DATETIME))'
                  WHERE     @toIssuingDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate >= CAST(@fromRequestDate AS DATE)'
                  WHERE     @fromRequestDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate <= DATEADD(MILLISECOND, -3, CAST(DATEADD(DAY, 1, CAST(@toRequestDate AS DATE)) AS DATETIME))'
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
