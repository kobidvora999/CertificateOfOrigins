namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class SaveCertificateAttachmentsArgsDto
{
    public List<TemplateResultDto> CertificatesTemplates { get; set; } = new();
    public string? CertificateNumber { get; set; }
    public int CertificateId { get; set; }
    public int CertificateRequestReasonCode { get; set; }
    public int CertificateTypeId { get; set; }
    public string? AdditionalInfo { get; set; }
}

// Mirrors Customs.Inf.CommonService TemplateResult — the already-rendered template payload.
public class TemplateResultDto
{
    public int DocumentTypeId { get; set; }
    public byte[]? Content { get; set; }
    public string? FileName { get; set; }
}
