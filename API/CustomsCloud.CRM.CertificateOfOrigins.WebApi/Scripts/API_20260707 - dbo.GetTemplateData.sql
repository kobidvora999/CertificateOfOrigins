-- dbo.GetTemplateData — template data for certificate-of-origin printing (uniform template flow).
-- TODO(developer): skeleton dataset only — extend to the full print dataset per template.
-- Contract: columns must match CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs.CertificateOfOriginPrintResult.
IF SCHEMA_ID(N'dbo') IS NULL EXEC(N'CREATE SCHEMA dbo');
GO
CREATE OR ALTER PROCEDURE dbo.GetTemplateData
    @TemplateId INT,
    @EntityId   INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.ID                            AS Id,
        c.CertificateNumber             AS CertificateNumber,
        c.TypeID                        AS TypeId,
        t.Name                          AS TypeName,
        c.CertificateOfOriginStatusID   AS CertificateOfOriginStatusId,
        c.IssuingDate                   AS IssuingDate,
        c.ExportDeclarationNumber       AS ExportDeclarationNumber,
        c.FeedbackRemark                AS FeedbackRemark,
        c.RejectCancelReason            AS RejectCancelReason,
        c.IsAttachedList                AS IsAttachedList,
        c.QRCodePath                    AS QrCodePath
    FROM CRM.CertificateOfOrigins_CertificateOfOrigin c
    LEFT JOIN CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode t ON t.ID = c.TypeID
    WHERE c.ID = @EntityId;
END
GO
