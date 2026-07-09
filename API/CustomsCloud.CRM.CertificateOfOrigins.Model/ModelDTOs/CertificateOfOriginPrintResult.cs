namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Data payload for certificate-of-origin template generation (dbo.GetTemplateData).
// TODO(developer): extend to the full print dataset per template — the SP body and this DTO
// must stay column-compatible (template-print-pattern rule).
public class CertificateOfOriginPrintResult
{
    public int Id { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;

    public int TypeId { get; set; }

    public string? TypeName { get; set; }

    public int CertificateOfOriginStatusId { get; set; }

    public DateTime? IssuingDate { get; set; }

    public string? ExportDeclarationNumber { get; set; }

    public string? FeedbackRemark { get; set; }

    public string? RejectCancelReason { get; set; }

    public bool IsAttachedList { get; set; }

    public string? QrCodePath { get; set; }
}
