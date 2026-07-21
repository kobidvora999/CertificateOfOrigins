---- CRM.CertificateOfOrigins_c_OriginCriterion ----

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_c_OriginCriterion'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_c_OriginCriterion] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_c_OriginCriterion_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

 
ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion] ADD  CONSTRAINT [DF_CertificateOfOrigins_c_OriginCriterion_State]  DEFAULT ((1)) FOR [State]

 
ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_c_OriginCriterion_CertificateOfOrigins_enum_CertificateOfOriginTypeCode] FOREIGN KEY([CertificateOfOriginTypeCodeID])
REFERENCES [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ([ID])

 
--ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion] CHECK CONSTRAINT [FK_CertificateOfOrigins_c_OriginCriterion_CertificateOfOrigins_enum_CertificateOfOriginTypeCode]
--GO
 
--ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_c_OriginCriterion_General_enum_State] FOREIGN KEY([State])
--REFERENCES [Shared].[General_enum_State] ([ID])
--GO
 
--ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion] CHECK CONSTRAINT [FK_CertificateOfOrigins_c_OriginCriterion_General_enum_State]
--GO
 
ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_c_OriginCriterion_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

 
ALTER TABLE [CRM].[CertificateOfOrigins_c_OriginCriterion] CHECK CONSTRAINT [CK_CertificateOfOrigins_c_OriginCriterion_ValidDate]

end
 
 ---- CRM.CertificateOfOrigins_c_VerificationProhibitedImporters ----

 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_c_VerificationProhibitedImporters'
      AND s.name = 'CRM'
)
BEGIN
 CREATE TABLE [CRM].[CertificateOfOrigins_c_VerificationProhibitedImporters](
	[ID] [int] NOT NULL,
	[CustomerId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) 
 
END

---- CRM.CertificateOfOrigins_CertificateOfOrigin ----

 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CertificateOfOrigin'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOrigin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
 
ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_QRCodePath]  DEFAULT ((1)) FOR [QRCodePath]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_IsAttachedList]  DEFAULT ((0)) FOR [IsAttachedList]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_VersionNumber]  DEFAULT ((0)) FOR [VersionNumber]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOrigin_IsLastVersion]  DEFAULT ((0)) FOR [IsLastVersion]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOrigin] ADD  DEFAULT ((0)) FOR [IsInPublishingProcess]

END


----- CertificateOfOrigins_CertificateOfOriginDetails ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CertificateOfOriginDetails'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CertificateOfOriginID] [int] NOT NULL,
	[CertificateDetailsTypeCodeID] [int] NOT NULL,
	[Value] [nvarchar](255) NULL,
	[DisplayedValue] [nvarchar](255) NULL,
CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginDetails] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginDetails_CertificateOfOrigins_CertificateOfOrigin] FOREIGN KEY([CertificateOfOriginID])
REFERENCES [CRM].[CertificateOfOrigins_CertificateOfOrigin] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginDetails_CertificateOfOrigins_CertificateOfOrigin]

--ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginDetails_CertificateOfOrigins_enum_CertificateDetailsTypeCode] FOREIGN KEY([CertificateDetailsTypeCodeID])
--REFERENCES [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginDetails_CertificateOfOrigins_enum_CertificateDetailsTypeCode]
END

----- CertificateOfOrigins_CertificateOfOriginInvoiceDetail ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CertificateOfOriginInvoiceDetail'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginInvoiceDetail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOriginInvoiceDetail_InvoiceAmount]  DEFAULT ((0)) FOR [InvoiceAmount]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail] ADD  CONSTRAINT [DF_CertificateOfOrigins_CertificateOfOriginInvoiceDetail_IsToPrint]  DEFAULT ((0)) FOR [IsToPrint]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginInvoiceDetail_CertificateOfOrigins_CertificateOfOrigin] FOREIGN KEY([CertificateOfOriginID])
REFERENCES [CRM].[CertificateOfOrigins_CertificateOfOrigin] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginInvoiceDetail_CertificateOfOrigins_CertificateOfOrigin]
END

----- CertificateOfOrigins_CertificateOfOriginItemDetail ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CertificateOfOriginItemDetail'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginItemDetail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginItemDetail_CertificateOfOrigins_c_OriginCriterion] FOREIGN KEY([OriginCriterionID])
REFERENCES [CRM].[CertificateOfOrigins_c_OriginCriterion] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginItemDetail] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginItemDetail_CertificateOfOrigins_c_OriginCriterion]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginItemDetail]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginItemDetail_CertificateOfOrigins_CertificateOfOriginInvoiceDetail] FOREIGN KEY([CertificateOfOriginInvoiceDetailID])
REFERENCES [CRM].[CertificateOfOrigins_CertificateOfOriginInvoiceDetail] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginItemDetail] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginItemDetail_CertificateOfOrigins_CertificateOfOriginInvoiceDetail]
END

----- CertificateOfOrigins_CertificateOfOriginVsDeclarationError ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_CertificateOfOriginVsDeclarationError'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginVsDeclarationError](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CertificateOfOriginID] [int] NOT NULL,
	[ErrorText] [nvarchar](512) NULL,
	[State] [int] NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_CertificateOfOriginVsDeclarationError] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginVsDeclarationError] ADD  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginVsDeclarationError]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginVsDeclarationError_CertificateOfOrigins_CertificateOfOrigin] FOREIGN KEY([CertificateOfOriginID])
REFERENCES [CRM].[CertificateOfOrigins_CertificateOfOrigin] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_CertificateOfOriginVsDeclarationError] CHECK CONSTRAINT [FK_CertificateOfOrigins_CertificateOfOriginVsDeclarationError_CertificateOfOrigins_CertificateOfOrigin]
END


----- CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CertificateOfOriginTypeCodeID] [int] NOT NULL,
	[TradeAgreementID] [int] NOT NULL,
	[ValidFrom] [datetime] NOT NULL,
	[ValidTo] [datetime] NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_cf_CertificateOfOriginTypeByTradeAgreement] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END

----- CertificateOfOrigins_cf_SupplierDeliveryCountryConfig ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_cf_SupplierDeliveryCountryConfig'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_cf_SupplierDeliveryCountryConfig](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ConutryID] [int] NOT NULL,
	[State] [int] NOT NULL
)

ALTER TABLE [CRM].[CertificateOfOrigins_cf_SupplierDeliveryCountryConfig] ADD  CONSTRAINT [DF_CertificateOfOrigins_cf_SupplierDeliveryCountryConfig_State]  DEFAULT ((0)) FOR [State]
END

----- CertificateOfOrigins_cl_DetailsPerCertificate ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_cl_DetailsPerCertificate'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CertificateOfOriginTypeCodeID] [int] NOT NULL,
	[ConstraintTypeEnumID] [int] NOT NULL,
	[CertificateDetailsTypeCodeID] [int] NOT NULL,
	[Order] [int] NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_cl_DetailsPerCertificate] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOriginTypeCodeIDConstraintTypeEnumIDCertificateDetailsTypeCodeID] UNIQUE NONCLUSTERED 
(
	[CertificateOfOriginTypeCodeID] ASC,
	[ConstraintTypeEnumID] ASC,
	[CertificateDetailsTypeCodeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_CertificateDetailsTypeCode] FOREIGN KEY([CertificateDetailsTypeCodeID])
REFERENCES [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate] CHECK CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_CertificateDetailsTypeCode]

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_CertificateOfOriginTypeCode] FOREIGN KEY([CertificateOfOriginTypeCodeID])
REFERENCES [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate] CHECK CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_CertificateOfOriginTypeCode]

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_ConstraintTypeEnum] FOREIGN KEY([ConstraintTypeEnumID])
REFERENCES [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_cl_DetailsPerCertificate] CHECK CONSTRAINT [FK_CertificateOfOrigins_cl_DetailsPerCertificate_CertificateOfOrigins_enum_ConstraintTypeEnum]
END

----- CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExportAuthenticationRequestID] [int] NOT NULL,
	[ManufacturingArea] [nvarchar](255) NULL,
	[ManufacturingZipcode] [nvarchar](255) NULL,
CONSTRAINT [PK_CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
) 

ALTER TABLE [CRM].[CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea_CertificateOfOrigins_ExportDocumentAuthenticationRequest] FOREIGN KEY([ExportAuthenticationRequestID])
REFERENCES [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea] CHECK CONSTRAINT [FK_CertificateOfOrigins_cl_ExportAuthenticationRequestManufacturingArea_CertificateOfOrigins_ExportDocumentAuthenticationRequest]
END

----- CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument ----
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExportRequestID] [int] NOT NULL,
	[LeadDocumentID] [int] NULL,
	[LeadDocumentTitle] [nvarchar](14) NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument_ExportDocumentAuthenticationRequest] FOREIGN KEY([ExportRequestID])
REFERENCES [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument] CHECK CONSTRAINT [FK_CertificateOfOrigins_cl_ExportDocumentAuthenticationRequestLeadDocument_ExportDocumentAuthenticationRequest]
END

----- CertificateOfOrigins_enum_AuthenticationFileStatus ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_AuthenticationFileStatus'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_AuthenticationFileStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_AuthenticationFileStatus_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_AuthenticationFileStatus_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_AuthenticationFileStatus_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_AuthenticationFileStatus_IsAutomatic]  DEFAULT ((0)) FOR [IsAutomatic]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_AuthenticationFileStatus_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_AuthenticationFileStatus_ValidDate]
END

----- CertificateOfOrigins_enum_CertificateDetailsTypeCode ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_CertificateDetailsTypeCode'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateDetailsTypeCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateDetailsTypeCode_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateDetailsTypeCode_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateDetailsTypeCode_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateDetailsTypeCode_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateDetailsTypeCode] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateDetailsTypeCode_ValidDate]
END

----- CertificateOfOrigins_enum_CertificateOfOriginStatusCode ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_CertificateOfOriginStatusCode'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateOfOriginStatusCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_State]  DEFAULT ((1)) FOR [State]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_RecordEditable]  DEFAULT ((0)) FOR [RecordEditable]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_UserPermitted]  DEFAULT ((0)) FOR [UserPermitted]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_IsRecordEditableNotExport]  DEFAULT ((0)) FOR [IsRecordEditableNotExport]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_IsUserPermittedNotExport]  DEFAULT ((0)) FOR [IsUserPermittedNotExport]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginStatusCode] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateOfOriginStatusCode_ValidDate]
END

----- CertificateOfOrigins_enum_CertificateOfOriginTypeCode ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_CertificateOfOriginTypeCode'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_CertificateOfOriginTypeCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_IsCriterionMandatory]  DEFAULT ((0)) FOR [IsCriterionMandatory]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_IsCustomApprovalRequired]  DEFAULT ((0)) FOR [IsCustomApprovalRequired]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ADD  DEFAULT ('false') FOR [IsCustomsItemMandatory]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_IsZipcodeMandatory]  DEFAULT ((0)) FOR [IsZipcodeMandatory]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CertificateOfOriginTypeCode] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_CertificateOfOriginTypeCode_ValidDate]
END

----- CertificateOfOrigins_enum_Circumstances ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_Circumstances'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_Circumstances] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_Circumstances_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_Circumstances_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)

ALTER TABLE [CRM].[CertificateOfOrigins_enum_Circumstances] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_Circumstances_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_Circumstances]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_Circumstances_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_Circumstances] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_Circumstances_ValidDate]
END

----- CertificateOfOrigins_enum_ConstraintTypeEnum ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_ConstraintTypeEnum'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_ConstraintTypeEnum] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_ConstraintTypeEnum_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_ConstraintTypeEnum_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_ConstraintTypeEnum_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_ConstraintTypeEnum_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_ConstraintTypeEnum] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_ConstraintTypeEnum_ValidDate]
 
END

----- CertificateOfOrigins_enum_CustomHouse ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_CustomHouse'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_CustomHouse] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_CustomHouse_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_CustomHouse_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CustomHouse] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_CustomHouse_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CustomHouse] ADD  DEFAULT ((4)) FOR [CountryID]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_CustomHouse]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_CustomHouse_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_CustomHouse] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_CustomHouse_ValidDate]
END

----- CertificateOfOrigins_enum_Decision ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_Decision'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_Decision] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_Decision_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_Decision_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
) 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_Decision_State]  DEFAULT ((1)) FOR [State]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_Decision_IsAutomatic]  DEFAULT ((0)) FOR [IsAutomatic]
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_Decision_IsForCoordinator]  DEFAULT ((0)) FOR [IsForCoordinator]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_Decision_IsForClaliMakorWorker]  DEFAULT ((0)) FOR [IsForClaliMakorWorker]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_Decision_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_Decision] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_Decision_ValidDate]
END

----- CertificateOfOrigins_enum_DeliveryMethod ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_DeliveryMethod'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_DeliveryMethod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_DeliveryMethod_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_DeliveryMethod_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
) 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_DeliveryMethod] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_DeliveryMethod_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_DeliveryMethod]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_DeliveryMethod_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_DeliveryMethod] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_DeliveryMethod_ValidDate]
END


----- CertificateOfOrigins_enum_ExportAuthenticationRequestStatus ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_ExportAuthenticationRequestStatus'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ,
CONSTRAINT [UQ_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
)
ALTER TABLE [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))
 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_ExportAuthenticationRequestStatus_ValidDate]
END

----- CertificateOfOrigins_enum_ImporterContactingReason ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_ImporterContactingReason'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_ImporterContactingReason] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_ImporterContactingReason_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_ImporterContactingReason_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ImporterContactingReason] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_ImporterContactingReason_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ImporterContactingReason]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_ImporterContactingReason_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ImporterContactingReason] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_ImporterContactingReason_ValidDate]
END

----- CertificateOfOrigins_enum_PrefernceDocumentType ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_PrefernceDocumentType'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_PrefernceDocumentType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_PrefernceDocumentType_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_PrefernceDocumentType_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
) 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_PrefernceDocumentType_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_PrefernceDocumentType_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_PrefernceDocumentType_ValidDate]
END

----- CertificateOfOrigins_enum_ReminderMethod ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_ReminderMethod'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_ReminderMethod] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_ReminderMethod_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
CONSTRAINT [UQ_CertificateOfOrigins_enum_ReminderMethod_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
) 
ALTER TABLE [CRM].[CertificateOfOrigins_enum_ReminderMethod] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_ReminderMethod_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ReminderMethod]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_ReminderMethod_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_ReminderMethod] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_ReminderMethod_ValidDate]
END


----- CertificateOfOrigins_enum_RequestReasonCode ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_enum_RequestReasonCode'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_enum_RequestReasonCode] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_RequestReasonCode_Enumeration] UNIQUE NONCLUSTERED 
(
	[Enumeration] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
CONSTRAINT [UQ_CertificateOfOrigins_enum_RequestReasonCode_Name] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_RequestReasonCode] ADD  CONSTRAINT [DF_CertificateOfOrigins_enum_RequestReasonCode_State]  DEFAULT ((1)) FOR [State]

ALTER TABLE [CRM].[CertificateOfOrigins_enum_RequestReasonCode]  WITH CHECK ADD  CONSTRAINT [CK_CertificateOfOrigins_enum_RequestReasonCode_ValidDate] CHECK  ((isnull([StartDate],'19000101')<isnull([EndDate],'99991231')))

ALTER TABLE [CRM].[CertificateOfOrigins_enum_RequestReasonCode] CHECK CONSTRAINT [CK_CertificateOfOrigins_enum_RequestReasonCode_ValidDate]
END

----- CertificateOfOrigins_ExportDocumentAuthenticationRequest ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_ExportDocumentAuthenticationRequest'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_ExportDocumentAuthenticationRequest] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_CertificateOfOrigins_enum_DeliveryMethod] FOREIGN KEY([DeliveryMethodID])
REFERENCES [CRM].[CertificateOfOrigins_enum_DeliveryMethod] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_CertificateOfOrigins_enum_DeliveryMethod]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_CertificateOfOrigins_enum_PrefernceDocumentType] FOREIGN KEY([AuthenticationDocumentTypeID])
REFERENCES [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_CertificateOfOrigins_enum_PrefernceDocumentType]

ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_enum_ExportAuthenticationRequestStatus] FOREIGN KEY([StatusID])
REFERENCES [CRM].[CertificateOfOrigins_enum_ExportAuthenticationRequestStatus] ([ID])
 
ALTER TABLE [CRM].[CertificateOfOrigins_ExportDocumentAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ExportDocumentAuthenticationRequest_enum_ExportAuthenticationRequestStatus]
END

----- CertificateOfOrigins_ImportAuthenticationFileDetails ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_ImportAuthenticationFileDetails'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_ImportAuthenticationFileDetails] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
) 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] ADD  CONSTRAINT [DF_CertificateOfOrigins_ImportAuthenticationFileDetails_State]  DEFAULT ((1)) FOR [State]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] ADD  CONSTRAINT [DF_CertificateOfOrigins_ImportAuthenticationFileDetails_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_AuthenticationFileStatus] FOREIGN KEY([AuthenticationFileStatusID])
REFERENCES [CRM].[CertificateOfOrigins_enum_AuthenticationFileStatus] ([ID])
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_AuthenticationFileStatus]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_DeliveryMethod] FOREIGN KEY([DeliveryMethodID])
REFERENCES [CRM].[CertificateOfOrigins_enum_DeliveryMethod] ([ID])
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_DeliveryMethod]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_ImporterContactingReason] FOREIGN KEY([ImporterContactingReasonID])
REFERENCES [CRM].[CertificateOfOrigins_enum_ImporterContactingReason] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_ImporterContactingReason]
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_ReminderMethod] FOREIGN KEY([ReminderMethodID])
REFERENCES [CRM].[CertificateOfOrigins_enum_ReminderMethod] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationFileDetails_CertificateOfOrigins_enum_ReminderMethod]
END

----- CertificateOfOrigins_ImportAuthenticationRequest ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_ImportAuthenticationRequest'
      AND s.name = 'CRM'
)
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
CONSTRAINT [PK_CertificateOfOrigins_ImportAuthenticationRequest] PRIMARY KEY CLUSTERED 
(
	[DocumentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
) 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] ADD  CONSTRAINT [DF_CertificateOfOrigins_ImportAuthenticationRequest_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] ADD  CONSTRAINT [DF_CertificateOfOrigins_ImportAuthenticationRequest_IsOldIndication]  DEFAULT ((0)) FOR [IsOldIndication]

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_authenticationFile] FOREIGN KEY([AuthenticationFileID])
REFERENCES [CRM].[CertificateOfOrigins_ImportAuthenticationFileDetails] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_authenticationFile]

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_Circumstances] FOREIGN KEY([RequestCircumstancesID])
REFERENCES [CRM].[CertificateOfOrigins_enum_Circumstances] ([ID])
 
ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_Circumstances]

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_Decision] FOREIGN KEY([DecisionID])
REFERENCES [CRM].[CertificateOfOrigins_enum_Decision] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_Decision]

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_PrefernceDocumentType] FOREIGN KEY([PreferenceDocumentTypeID])
REFERENCES [CRM].[CertificateOfOrigins_enum_PrefernceDocumentType] ([ID])

ALTER TABLE [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] CHECK CONSTRAINT [FK_CertificateOfOrigins_ImportAuthenticationRequest_CertificateOfOrigins_enum_PrefernceDocumentType]
END


----- CertificateOfOrigins_ItemDetails ----
 IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'CertificateOfOrigins_ItemDetails'
      AND s.name = 'CRM'
)
BEGIN
CREATE TABLE [CRM].[CertificateOfOrigins_ItemDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ImportAuthenticationRequestID] [int] NULL,
	[CustomItemID] [int] NOT NULL,
CONSTRAINT [PK_CertificateOfOrigins_ItemDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [CRM].[CertificateOfOrigins_ItemDetails]  WITH CHECK ADD  CONSTRAINT [FK_CertificateOfOrigins_ItemDetails_Id_CertificateOfOrigins_ImportAuthenticationRequest] FOREIGN KEY([ImportAuthenticationRequestID])
REFERENCES [CRM].[CertificateOfOrigins_ImportAuthenticationRequest] ([DocumentID])

ALTER TABLE [CRM].[CertificateOfOrigins_ItemDetails] CHECK CONSTRAINT [FK_CertificateOfOrigins_ItemDetails_Id_CertificateOfOrigins_ImportAuthenticationRequest]
END

