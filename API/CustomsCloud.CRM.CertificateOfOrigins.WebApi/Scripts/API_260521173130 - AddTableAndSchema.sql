-- =============================================
-- Author:      TEAM\kobi_dv
-- Create date: 21/05/2026 17:31:30
-- Description: <TODO: add description>
-- Approved by:
-- =============================================
-- Update date: <Date> - <Name> - <Description>
-- =============================================

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- CertificateOfOrigins_c_OriginCriterion
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_c_OriginCriterion')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [CertificateOfOriginTypeCodeID] [int] NOT NULL,
        [OriginCriterionCode] [nvarchar](3) NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_c_OriginCriterion] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_c_OriginCriterion_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_c_VerificationProhibitedImporters
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_c_VerificationProhibitedImporters')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_c_VerificationProhibitedImporters](
        [ID] [int] NOT NULL,
        [CustomerId] [int] NOT NULL,
        PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CertificateOfOrigin
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CertificateOfOrigin')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [TypeID] [int] NOT NULL,
        [Title] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [TimeStamp] [timestamp] NOT NULL,
        [CreateDate] [datetime] NOT NULL,
        [CreateUserID] [int] NOT NULL,
        [UpdateDate] [datetime] NOT NULL,
        [UpdateUserID] [int] NOT NULL,
        [OrganizationUnitID] [int] NOT NULL,
        [CustomerID] [int] NOT NULL,
        [CreateCustomerID] [int] NOT NULL,
        [UpdateCustomerID] [int] NOT NULL,
        [LeadDocumentID] [int] NULL,
        [CertificateIDToCancel] [int] NULL,
        [CertificateNumber] [nvarchar](35) NOT NULL,
        [CertificateOfOriginStatusID] [int] NOT NULL,
        [DestinationCountry] [int] NULL,
        [FeedbackRemark] [nvarchar](255) NULL,
        [InternalApplication] [nvarchar](35) NULL,
        [IssuingDate] [datetime] NULL,
        [RejectCancelReason] [nvarchar](512) NULL,
        [ReplacementReason] [nvarchar](255) NULL,
        [RequestReasonCode] [int] NOT NULL,
        [ExportDeclarationNumber] [nvarchar](17) NULL,
        [CertificateToReplaceInImport] [nvarchar](255) NULL,
        [GUID] [uniqueidentifier] NULL,
        [QRCodePath] [nvarchar](1000) NULL,
        [IsAttachedList] [bit] NOT NULL,
        [InSufficentworkingInd] [bit] NULL,
        [InsufficentWorkingText] [nvarchar](255) NULL,
        [QrImage] [varbinary](max) NULL,
        [ApproveUserID] [int] NULL,
        [VersionNumber] [int] NOT NULL,
        [IsLastVersion] [bit] NOT NULL,
        [IsInPublishingProcess] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOrigin] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CertificateOfOriginDetails
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CertificateOfOriginDetails')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [CertificateOfOriginID] [int] NOT NULL,
        [CertificateDetailsTypeCodeID] [int] NOT NULL,
        [Value] [nvarchar](255) NULL,
        [DisplayedValue] [nvarchar](255) NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginDetails] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CertificateOfOriginInvoiceDetail
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CertificateOfOriginInvoiceDetail')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [CertificateOfOriginID] [int] NOT NULL,
        [CurrencyTypeID] [int] NULL,
        [InvoiceAmount] [money] NOT NULL,
        [InvoiceDate] [datetime] NOT NULL,
        [InvoiceGoodsDescription] [nvarchar](255) NOT NULL,
        [InvoiceNumber] [nvarchar](35) NOT NULL,
        [IsToPrint] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginInvoiceDetail] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CertificateOfOriginItemDetail
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CertificateOfOriginItemDetail')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginItemDetail](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [PackingTypeID] [int] NULL,
        [CustomsItemID] [int] NULL,
        [GrossWeight] [numeric](10, 2) NOT NULL,
        [CertificateOfOriginInvoiceDetailID] [int] NOT NULL,
        [ItemGoodsDescription] [nvarchar](4000) NOT NULL,
        [MarksAndNumbers] [nvarchar](255) NOT NULL,
        [MeasurementUnitID] [int] NOT NULL,
        [OriginCriterionID] [int] NULL,
        [Quantity] [int] NOT NULL,
        [RowNum] [int] NOT NULL,
        [FullClassification] [nvarchar](13) NOT NULL,
        [ContainerISOCode] [nvarchar](20) NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginItemDetail] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CertificateOfOriginVsDeclarationError
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CertificateOfOriginVsDeclarationError')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginVsDeclarationError](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [CertificateOfOriginID] [int] NOT NULL,
        [ErrorText] [nvarchar](512) NULL,
        [State] [int] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginVsDeclarationError] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [CertificateOfOriginTypeCodeID] [int] NOT NULL,
        [TradeAgreementID] [int] NOT NULL,
        [ValidFrom] [datetime] NOT NULL,
        [ValidTo] [datetime] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_cf_SupplierDeliveryCountryConfig
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_cf_SupplierDeliveryCountryConfig')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_cf_SupplierDeliveryCountryConfig](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [ConutryID] [int] NOT NULL,
        [State] [int] NOT NULL
    )
END
GO

-- =============================================
-- CertificateOfOrigins_cl_DetailsPerCertificate
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_cl_DetailsPerCertificate')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [CertificateOfOriginTypeCodeID] [int] NOT NULL,
        [ConstraintTypeEnumID] [int] NOT NULL,
        [CertificateDetailsTypeCodeID] [int] NOT NULL,
        [Order] [int] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_cl_DetailsPerCertificate] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOriginTypeCodeIDConstraintTypeEnumIDCertificateDetailsTypeCodeID] UNIQUE NONCLUSTERED ([CertificateOfOriginTypeCodeID] ASC, [ConstraintTypeEnumID] ASC, [CertificateDetailsTypeCodeID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [ExportAuthenticationRequestID] [int] NOT NULL,
        [ManufacturingArea] [nvarchar](255) NULL,
        [ManufacturingZipcode] [nvarchar](255) NULL,
        CONSTRAINT [PK_CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [ExportRequestID] [int] NOT NULL,
        [LeadDocumentID] [int] NULL,
        [LeadDocumentTitle] [nvarchar](14) NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument] PRIMARY KEY NONCLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [ExportDocumentAuthenticationRequestID] [int] NOT NULL,
        [CustomsItemID] [int] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_CustomsItemToExportDocumentAuthenticationRequest] UNIQUE NONCLUSTERED ([ExportDocumentAuthenticationRequestID] ASC, [CustomsItemID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_AuthenticationFileStatus
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_AuthenticationFileStatus')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [IsAutomatic] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_AuthenticationFileStatus] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_AuthenticationFileStatus_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_AuthenticationFileStatus_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_CertificateDetailsTypeCode
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_CertificateDetailsTypeCode')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [Comment] [nvarchar](255) NULL,
        [DetailTypeFormat] [nvarchar](255) NULL,
        [DataTypeID] [int] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateDetailsTypeCode] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateDetailsTypeCode_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateDetailsTypeCode_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_CertificateOfOriginStatusCode
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_CertificateOfOriginStatusCode')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [RecordEditable] [bit] NOT NULL,
        [UserPermitted] [bit] NOT NULL,
        [IsRecordEditableNotExport] [bit] NOT NULL,
        [IsUserPermittedNotExport] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateOfOriginStatusCode] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_CertificateOfOriginTypeCode
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_CertificateOfOriginTypeCode')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [IsCriterionMandatory] [bit] NOT NULL,
        [IsCustomApprovalRequired] [bit] NOT NULL,
        [ReportId] [int] NULL,
        [IsCustomsItemMandatory] [bit] NULL,
        [IsZipcodeMandatory] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateOfOriginTypeCode] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_Circumstances
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_Circumstances')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_Circumstances](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_Circumstances] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_Circumstances_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_Circumstances_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_ConstraintTypeEnum
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_ConstraintTypeEnum')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_ConstraintTypeEnum] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ConstraintTypeEnum_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ConstraintTypeEnum_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_CustomHouse
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_CustomHouse')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_CustomHouse](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [customHouseAddress] [nvarchar](255) NULL,
        [CountryID] [int] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_CustomHouse] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CustomHouse_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_CustomHouse_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_Decision
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_Decision')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_Decision](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [IsAutomatic] [bit] NOT NULL,
        [IsForCoordinator] [bit] NOT NULL,
        [IsForClaliMakorWorker] [bit] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_Decision] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_Decision_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_Decision_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_DeliveryMethod
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_DeliveryMethod')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_DeliveryMethod](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_DeliveryMethod] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_DeliveryMethod_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_DeliveryMethod_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_ExportAuthenticationRequestStatus
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_ExportAuthenticationRequestStatus')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_ImporterContactingReason
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_ImporterContactingReason')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_ImporterContactingReason](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_ImporterContactingReason] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ImporterContactingReason_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ImporterContactingReason_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_enum_PrefernceDocumentType
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_PrefernceDocumentType')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_PrefernceDocumentType] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_PrefernceDocumentType_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_PrefernceDocumentType_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_ReminderMethod
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_ReminderMethod')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_ReminderMethod](
        [ID] [int] NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        [ReminderDate] [nvarchar](255) NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_ReminderMethod] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ReminderMethod_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
        CONSTRAINT [UQ_CertificateOfOrigins_enum_ReminderMethod_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_enum_RequestReasonCode
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_enum_RequestReasonCode')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_enum_RequestReasonCode](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](255) NOT NULL,
        [State] [int] NOT NULL,
        [Description] [nvarchar](255) NULL,
        [EnglishName] [nvarchar](255) NOT NULL,
        [Enumeration] [nvarchar](255) NOT NULL,
        [StartDate] [datetime] NULL,
        [EndDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_enum_RequestReasonCode] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_RequestReasonCode_Enumeration] UNIQUE NONCLUSTERED ([Enumeration] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
        CONSTRAINT [UQ_CertificateOfOrigins_enum_RequestReasonCode_Name] UNIQUE NONCLUSTERED ([Name] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_ExportDocumentAuthenticationRequest
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_ExportDocumentAuthenticationRequest')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [TypeID] [int] NOT NULL,
        [Title] [nvarchar](25) NOT NULL,
        [State] [int] NOT NULL,
        [TimeStamp] [timestamp] NOT NULL,
        [CreateDate] [datetime] NOT NULL,
        [CreateUserID] [int] NOT NULL,
        [UpdateDate] [datetime] NOT NULL,
        [UpdateUserID] [int] NOT NULL,
        [OrganizationUnitID] [int] NOT NULL,
        [CustomerID] [int] NOT NULL,
        [AuthenticationDocumentTypeID] [int] NOT NULL,
        [ExporterCustomerID] [int] NULL,
        [StatusID] [int] NULL,
        [CountryID] [int] NULL,
        [CustomsHouseAddress] [nvarchar](1000) NULL,
        [VendorID] [int] NULL,
        [AuthenticationRequestArrivalDate] [datetime] NULL,
        [AuthenticationRequestedByName] [nvarchar](255) NULL,
        [AuthenticationRequestedByEmail] [nvarchar](255) NULL,
        [AuthenticationRequestedByPhone] [nvarchar](14) NULL,
        [AuthenticationRequestNotes] [nvarchar](255) NOT NULL,
        [ExportLeadDocumentID] [int] NULL,
        [DocumentID] [int] NULL,
        [MainDocumentTitle] [nvarchar](255) NULL,
        [LastDeliveryDate] [datetime] NULL,
        [DeliveryMethodID] [int] NULL,
        [InvoiceNumbers] [nvarchar](600) NULL,
        [DetailedDecision] [nvarchar](1000) NULL,
        [ReferenceNumber] [nvarchar](255) NULL,
        [CommentForCustomsHouseLetter] [nvarchar](1000) NULL,
        [TotalDocuments] [int] NULL,
        [TotalInvoices] [int] NULL,
        [DocumentDate] [datetime] NULL,
        [InvoiceDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_ExportDocumentAuthenticationRequest] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO

-- =============================================
-- CertificateOfOrigins_ImportAuthenticationFileDetails
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_ImportAuthenticationFileDetails')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails](
        [ID] [int] IDENTITY(1,1) NOT NULL,
        [State] [int] NOT NULL,
        [TimeStamp] [timestamp] NOT NULL,
        [CreateDate] [datetime] NOT NULL,
        [CreateUserID] [int] NOT NULL,
        [UpdateDate] [datetime] NOT NULL,
        [UpdateUserID] [int] NOT NULL,
        [AuthenticationFileStatusID] [int] NOT NULL,
        [Notes] [nvarchar](512) NULL,
        [PostalAdress] [nvarchar](255) NOT NULL,
        [DeliveryMethodID] [int] NOT NULL,
        [EmailAdress] [nvarchar](255) NULL,
        [ReminderMethodID] [int] NOT NULL,
        [RequestCountryID] [int] NOT NULL,
        [UserID] [int] NOT NULL,
        [UserNameIssuingLetter] [nvarchar](255) NOT NULL,
        [LastDelivery] [datetime] NULL,
        [ImporterContactingReasonID] [int] NULL,
        [FirstProvideContactDate] [datetime] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_ImportAuthenticationFileDetails] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_ImportAuthenticationRequest
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_ImportAuthenticationRequest')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest](
        [DocumentID] [int] NOT NULL,
        [CreateDate] [datetime] NOT NULL,
        [CreateUserID] [int] NOT NULL,
        [UpdateDate] [datetime] NOT NULL,
        [UpdateUserID] [int] NOT NULL,
        [AuthenticationFileID] [int] NULL,
        [AuthenticationRequestDate] [datetime] NOT NULL,
        [CirumstanceDetails] [nvarchar](512) NULL,
        [CollateralID] [int] NULL,
        [DecisionCircumstences] [nvarchar](512) NULL,
        [DecisionID] [int] NULL,
        [LeadDocumentID] [int] NOT NULL,
        [DocumentIssuingDate] [datetime] NOT NULL,
        [ImportCountryID] [int] NOT NULL,
        [IssuingCountryID] [int] NOT NULL,
        [ItemDetailID] [int] NOT NULL,
        [Number] [int] NOT NULL,
        [IsOldIndication] [bit] NOT NULL,
        [OriginCountryID] [int] NOT NULL,
        [PreferenceDocumentTypeID] [int] NOT NULL,
        [Remarks] [nvarchar](512) NOT NULL,
        [RequestCircumstancesID] [int] NOT NULL,
        [UserResponseID] [int] NOT NULL,
        [ResponseNameEmail] [nvarchar](255) NULL,
        [ResponsePhoneNum] [nvarchar](255) NULL,
        [OrganizationUnitID] [int] NOT NULL,
        [UserID] [int] NOT NULL,
        [VendorId] [int] NULL,
        [VendorName] [nvarchar](45) NULL,
        [OrganizationUnitTypeID] [int] NULL,
        [DocumentNumber] [nvarchar](255) NULL,
        [CustomerID] [int] NULL,
        [ImporterID] [int] NULL,
        [LastDeliveryForImporter] [datetime] NULL,
        [InvoiceNumber] [nvarchar](225) NULL,
        [InvoiceGoodsItemTaxDifference] [money] NULL,
        [AllInvoiceGoodsItemTaxDifference] [money] NULL,
        CONSTRAINT [PK_CertificateOfOrigins_ImportAuthenticationRequest] PRIMARY KEY CLUSTERED ([DocumentID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
    )
END
GO

-- =============================================
-- CertificateOfOrigins_ItemDetails
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'CRM' AND t.name = 'CertificateOfOrigins_ItemDetails')
BEGIN
    CREATE TABLE [CRM].[CertificateOfOrigins_ItemDetails](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [ImportAuthenticationRequestID] [int] NULL,
        [CustomItemID] [int] NOT NULL,
        CONSTRAINT [PK_CertificateOfOrigins_ItemDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END
GO
