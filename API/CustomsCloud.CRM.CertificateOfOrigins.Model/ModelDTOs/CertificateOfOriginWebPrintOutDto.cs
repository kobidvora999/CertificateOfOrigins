namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result set 5 of dbo.GetCertificateOfOriginDataForWebQuery — the per-detail web print-out (English label +
// displayed value). LEGACY QUIRK (preserved, developer-confirmed 2026-07-28): the SP does NOT return an
// IsToPrint column, so CertificateDetailsTypeIsToPrint is always false — Consignee fields for EUR1/EURMED are
// therefore never printed.
public class CertificateOfOriginWebPrintOutDto
{
    public int CertificateDetailsTypeId { get; set; }
    public string? CertificateDetailsTypeEnglishName { get; set; }
    public string? CertificateDetailsTypeValue { get; set; }
    public bool CertificateDetailsTypeIsToPrint { get; set; }
}
