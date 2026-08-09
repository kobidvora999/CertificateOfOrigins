namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The DealFile lead document linked to a certificate of origin (SaveCertificateOfOrigin). The BL compares
// LeadDocumentTitle to the certificate's ExportDeclarationNumber and backfills LeadDocumentId/ExportDeclarationNumber
// when the certificate has none. Mirrors the legacy LeadDocumentByCertificateOfOriginDTO.
public class LeadDocumentByCertificateOfOriginDto
{
    public int LeadDocumentId { get; set; }

    public string? LeadDocumentTitle { get; set; }
}
