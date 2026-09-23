-- CR 194221 — the service-wide template-data procedure.
--
-- One procedure per service (the template-print pattern): the caller passes a template id and the entity it is
-- rendered for, and the procedure returns that template's column set. Each new template adds a branch here plus a
-- matching Result DTO and a case in CertificateOfOriginsBl.GetTemplateMeta.
--
-- ⚠️ ENCODING: UTF-8 without a BOM (repo convention). Apply with `sqlcmd -f 65001 ...`, SSMS or Azure Data Studio;
-- plain `sqlcmd -i` reads it as ANSI and would corrupt any Hebrew literal added later.

IF OBJECT_ID('dbo.GetTemplateData', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetTemplateData;
GO

CREATE PROCEDURE dbo.GetTemplateData
    @TemplateID INT,
    @EntityID   INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1 = SouthKoreaOriginVerificationLetter (ECertificateOfOriginsTemplate).
    -- @EntityID is CRM.CertificateOfOrigins_ImportAuthenticationFileDetails.ID — the authentication file.
    IF @TemplateID = 1
    BEGIN
        SELECT
            FileNo               = F.ID,
            LetterDate           = CAST(GETDATE() AS DATE),
            MovementCertificates = Movement.Numbers,
            InvoiceDeclarations  = Invoice.Numbers
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

    -- An unregistered template id returns no rows; the BL turns that into a 404.
    RETURN;
END
GO
