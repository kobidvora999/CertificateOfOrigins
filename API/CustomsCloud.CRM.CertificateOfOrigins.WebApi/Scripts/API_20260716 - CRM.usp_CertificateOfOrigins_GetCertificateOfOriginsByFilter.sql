SET QUOTED_IDENTIFIER, ANSI_NULLS ON;
GO
 
CREATE OR ALTER PROCEDURE [CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter]
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
            @From NVARCHAR(4000) ,
            @Where NVARCHAR(4000) ,
            @OrderBy NVARCHAR(4000) ,
            @Filter NVARCHAR(4000) = '' ,
            @TableJoin NVARCHAR(4000) = ''    		
 
   	
        SET @Select = '   	
SELECT 	TOP (shared.ufn_GetMaxRows())
				F.ID, 
                F.CertificateNumber, 
                S.Name, 
                C.ID CustomesAgentID, 
                C.Title CustomesAgentTitle, 
                C.ExternalIdNum CustomesAgentExternalIdNum,                     
                E.ID ExporterID, 
                E.Title ExporterTitle, 
                E.ExternalIdNum ExporterExternalIdNum,  
                F.ExportDeclarationNumber,
				F.VersionNumber,
				F.OrganizationUnitID,
				F.RequestReasonCode,
				F.IssuingDate,
				F.LeadDocumentID
		'
        SET @From = '		
FROM    CRM.CertificateOfOrigins_CertificateOfOrigin F
        INNER JOIN Shared.rStockPileData_Customers_Customer E ON F.CustomerID = E.ID
        INNER JOIN Shared.rStockPileData_Customers_Customer C ON F.CreateCustomerID = C.ID
        INNER JOIN CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode S ON f.CertificateOfOriginStatusID = S.ID
    '		
        SET @Where = '	
WHERE	(F.State = 1)    			
    '	
		SET @certificateNumber = '%' + @certificateNumber + '%'
 
        SELECT  @Filter += CASE WHEN Filter != ''
                                THEN '		and (' + Filter + ')
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
                  SELECT    N'F.IssuingDate >= Shared.ufn_General_GetDateStart(@fromIssuingDate)' 
                  WHERE     @fromIssuingDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.IssuingDate <= Shared.ufn_General_GetDateEnd(@toIssuingDate)' 
                  WHERE     @toIssuingDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate >= Shared.ufn_General_GetDateStart(@fromRequestDate)' 
                  WHERE     @fromRequestDate IS NOT NULL
                  UNION ALL
                  SELECT    N'F.CreateDate <= Shared.ufn_General_GetDateEnd(@toRequestDate)' 
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
    --PRINT @Select
  /**/      
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