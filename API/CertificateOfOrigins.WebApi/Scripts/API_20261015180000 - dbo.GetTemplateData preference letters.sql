-- The service-wide template-data procedure, with the legacy preference-document letters.
--
-- One procedure per service (the template-print pattern): the caller passes a template id and the entity it is rendered
-- for, and the procedure returns that template's column set. Each template adds a branch here plus a Result DTO and a
-- case in CertificateOfOriginsBl.GetTemplateMeta. The signature is the full legacy Object Maker one (@EntityID,
-- @TemplateTypeID, @CreateUserID, @AdditionalInfo) — see the previous version, API_20261015120000.
--
-- The letter branches port the legacy data procedures. A service-owned procedure never joins another service's tables,
-- so the legacy joins to Vendors / Customers / Users / Docs / DealFile / the Country table move to the BL
-- (CertificateOfOriginsBl.EnrichTemplateData) and the procedure returns the ids they need. A moved INNER JOIN on a
-- nullable FK keeps its row-filtering here as an IS NOT NULL guard; one on a non-nullable FK filtered nothing and is dropped.
--
-- The template ids are the legacy ones (Infrastructure.General_enum_Template.ID), as in ECertificateOfOriginsTemplate.
--
-- ⚠️ ENCODING: UTF-8 without a BOM (repo convention). Apply with `sqlcmd -f 65001 ...`, SSMS or Azure Data Studio;
-- plain `sqlcmd -i` reads it as ANSI and would corrupt any Hebrew literal added later.

IF OBJECT_ID('dbo.GetTemplateData', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTemplateData;
GO

CREATE PROCEDURE dbo.GetTemplateData
    @EntityID       INT,
    @TemplateTypeID INT,
    @CreateUserID   INT,
    @AdditionalInfo NVARCHAR(4000)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1 = SouthKoreaOriginVerificationLetter (ECertificateOfOriginsTemplate).
    -- @EntityID is CRM.CertificateOfOrigins_ImportAuthenticationFileDetails.ID — the authentication file.
    IF @TemplateTypeID = 1
    BEGIN
        -- The column names are the sibling import-verification letters' tag vocabulary (see 2251), not ad-hoc ones.
        -- The signature block (EmployeeFullName / EmployeeEmailAddress / UserSignature) comes from the Users service and
        -- is filled in the BL.
        SELECT
            ImportAuthenticationRequestDate = CAST(GETDATE() AS DATE),
            FileNumber                      = F.ID,
            ExternalCustomDepartmentAddress = F.PostalAdress,
            RequestCountryId                = F.RequestCountryID,
            MovementCertificate             = Movement.Numbers,
            InvoiceNumberForImportAuthenticationRequest = Invoice.Numbers,
            -- FREETEXT is a reserved T-SQL keyword, so the alias must be bracketed.
            [FreeText]                      = F.Notes
        FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails AS F
        -- The child requests carry the preference document numbers; 4 = תעודת תנועה, 5 = הצהרת חשבונית
        -- (CRM.CertificateOfOrigins_enum_PrefernceDocumentType). Blank numbers are skipped so the letter does not
        -- render stray separators, and OUTER APPLY keeps the file row when a category has no documents at all.
        OUTER APPLY (
            SELECT Numbers = STRING_AGG(R.DocumentNumber, N', ') WITHIN GROUP (ORDER BY R.DocumentNumber)
            FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest AS R
            WHERE R.AuthenticationFileID = F.ID
              AND R.PreferenceDocumentTypeID = 4
              AND NULLIF(LTRIM(RTRIM(R.DocumentNumber)), N'') IS NOT NULL
        ) AS Movement
        OUTER APPLY (
            SELECT Numbers = STRING_AGG(R.DocumentNumber, N', ') WITHIN GROUP (ORDER BY R.DocumentNumber)
            FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest AS R
            WHERE R.AuthenticationFileID = F.ID
              AND R.PreferenceDocumentTypeID = 5
              AND NULLIF(LTRIM(RTRIM(R.DocumentNumber)), N'') IS NOT NULL
        ) AS Invoice
        WHERE F.ID = @EntityID
          AND F.State <> 99;   -- repo soft-delete convention

        RETURN;
    END

    -- 2251-2259 — the import-verification letters (ImportVerificationLetterResult).
    -- @EntityID is the authentication file. Legacy: Infrastructure.usp_Template_INNER_CROSS_CertificateOfOrigin, CR126557.
    IF @TemplateTypeID IN (2251, 2252, 2253, 2254, 2255, 2256, 2257, 2258, 2259)
    BEGIN
        -- Legacy grouped the file's requests by vendor (CROSS APPLY ... GROUP BY VendorId) and returned one DISTINCT row per
        -- vendor group and document; the merger read Rows[0], so a file with several vendors printed whichever group came
        -- first. That order was undefined; here it is the lowest vendor id (a group without a vendor last).
        --
        -- Per group: MovementCertificate lists each request's DocumentNumber — or its InvoiceNumber when the request is an
        -- invoice declaration (preference document type 5 / 6), except in 2251, 2252, 2256, 2257 — and
        -- InvoiceNumberForImportAuthenticationRequest lists the InvoiceNumbers. Legacy concatenated with the CLR aggregate
        -- Shared.ufn_Util_clr_Conc, each value cut to NVARCHAR(225).
        -- TODO(confirm): ufn_Util_clr_Conc's separator — ', ' is assumed (the South-Korea branch's separator).
        --
        -- The foreign customs department block (ExternalCustomDepartment*) came from each row's request customer
        -- (#CustomerDetails, LEFT JOINed on the request's CustomerID); with Rows[0] that was whichever request came first.
        -- Here it is the customer of the file's first request (lowest DocumentID) that has one. The BL resolves it.
        --
        -- Legacy INNER JOINed every request to Infrastructure.Docs_Document on DocumentID, which is this table's primary
        -- key — a non-nullable FK that filtered nothing, so it is dropped. A file with no requests has no row (404).
        SELECT TOP (1)
            ImportAuthenticationRequestDate = CONVERT(VARCHAR(12), GETDATE(), 107),
            FileNumber                      = F.ID,
            VendorId                        = V.VendorID,
            MovementCertificate             = V.MovementCertificate,
            InvoiceNumberForImportAuthenticationRequest = V.InvoiceNumbers,
            ReplyDate                       = CONVERT(VARCHAR(12), DATEADD(MONTH, 3, GETDATE()), 107),
            -- The reminder letter (2257) quotes the date of the first letter: the file's last delivery.
            FirstLetterDate                 = CONVERT(VARCHAR(12), F.LastDelivery, 107),
            CustomerId                      = (SELECT TOP (1) C.CustomerID
                                               FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest AS C
                                               WHERE C.AuthenticationFileID = F.ID
                                                 AND C.CustomerID IS NOT NULL
                                               ORDER BY C.DocumentID)
        FROM CRM.CertificateOfOrigins_ImportAuthenticationFileDetails AS F
        CROSS APPLY (
            SELECT
                R.VendorID,
                MovementCertificate = STRING_AGG(CAST(IIF(R.PreferenceDocumentTypeID IN (5, 6)
                                                              AND @TemplateTypeID NOT IN (2251, 2252, 2256, 2257),
                                                          R.InvoiceNumber,
                                                          R.DocumentNumber) AS NVARCHAR(225)), N', ')
                                      WITHIN GROUP (ORDER BY R.DocumentID),
                InvoiceNumbers      = STRING_AGG(CAST(R.InvoiceNumber AS NVARCHAR(225)), N', ')
                                      WITHIN GROUP (ORDER BY R.DocumentID)
            FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest AS R
            WHERE R.AuthenticationFileID = F.ID
            GROUP BY R.VendorID
        ) AS V
        WHERE F.ID = @EntityID
          AND F.State <> 99    -- repo soft-delete convention
        ORDER BY IIF(V.VendorID IS NULL, 1, 0), V.VendorID;

        RETURN;
    END

    -- 2244 — the importer alert letter (ImporterAlertLetterResult). @EntityID is the authentication request's DocumentID.
    -- Legacy: Infrastructure.usp_Template_INNER_CROSS_ImportAuthenticationRequestImporterLetter.
    IF @TemplateTypeID = 2244
    BEGIN
        -- Legacy INNER JOINed the importer (Customers_Customer) and the vendor (Vendors_Vendor + Vendors_VendorAddress) on
        -- nullable FKs — their guards stay. The lead document / its calculation data / declaration, Docs_Document and the
        -- producing user were INNER JOINs on non-nullable keys and filtered nothing here.
        SELECT
            ImportAuthenticationRequestID               = R.DocumentID,
            DocumentTitle                               = R.DocumentNumber,
            AuthenticationFileID                        = R.AuthenticationFileID,
            InvoiceNumberForImportAuthenticationRequest = R.InvoiceNumber,
            ImporterId                                  = R.ImporterID,
            LeadDocumentId                              = R.LeadDocumentID,
            UpdateUserId                                = R.UpdateUserID
        FROM CRM.CertificateOfOrigins_ImportAuthenticationRequest AS R
        INNER JOIN CRM.CertificateOfOrigins_ImportAuthenticationFileDetails AS F
            ON F.ID = R.AuthenticationFileID
        WHERE R.DocumentID = @EntityID
          AND F.State <> 99    -- repo soft-delete convention
          AND R.ImporterID IS NOT NULL
          AND R.VendorID IS NOT NULL;

        RETURN;
    END

    -- 2289-2295, 2317 — the export-verification letters (ExportVerificationLetterResult). @EntityID is the export authentication
    -- request. Legacy: Infrastructure.usp_Template_INNER_CROSS_ExportDocumentAuthenticationRequestLetter; both of its
    -- branches select the same columns and differ only in the signing user, which the BL resolves.
    -- (usp_Template_CROSS_ObjectMaker routes 2317, the empty template, to the same procedure.)
    IF @TemplateTypeID IN (2289, 2290, 2291, 2292, 2293, 2294, 2295, 2317)
    BEGIN
        -- Legacy INNER JOINed the country (Shared.General_c_Country), the document (Infrastructure.Docs_Document) and the
        -- exporter (Customers_Customer) on nullable FKs — their guards stay. The foreign customs department (CustomerID) is
        -- non-nullable; its activity-40 filter moves to the BL with the rest of the customer data.
        -- 2289's manufacturing areas (a second result set, TOP 4 from cl_ExportAuthenticationRequestManufacturingArea) fed a
        -- repeating-row content control the Templates module cannot merge, so they are not selected (see Templates/README.md).
        SELECT
            ExportAuthenticationRequestDate = CONVERT(VARCHAR(12), GETDATE(), 107),
            ReferenceNumber                 = R.ReferenceNumber,
            ExportRequestID                 = R.ID,
            CountryId                       = R.CountryID,
            DocumentDate                    = CONVERT(VARCHAR(12), R.DocumentDate, 101),
            InvoiceDate                     = CONVERT(VARCHAR(12), R.InvoiceDate, 101),
            MainDocumentTitle               = R.MainDocumentTitle,
            InvoiceNumbers                  = R.InvoiceNumbers,
            CustomsHouseAddress             = R.CustomsHouseAddress,
            CustomerId                      = R.CustomerID,
            ExporterCustomerId              = R.ExporterCustomerID,
            IsDocument                      = CAST(IIF(R.MainDocumentTitle IS NULL, 0, 1) AS BIT),
            IsInvoice                       = CAST(IIF(R.InvoiceNumbers IS NULL, 0, 1) AS BIT)
        FROM CRM.CertificateOfOrigins_ExportDocumentAuthenticationRequest AS R
        WHERE R.ID = @EntityID
          AND R.State <> 99    -- repo soft-delete convention
          AND R.CountryID IS NOT NULL
          AND R.DocumentID IS NOT NULL
          AND R.ExporterCustomerID IS NOT NULL;

        RETURN;
    END

    -- An unregistered template id returns no rows; the BL turns that into a 404 (an id this service does not
    -- render at all is rejected earlier, in GetTemplateMeta, as a bad request).
    RETURN;
END
GO
