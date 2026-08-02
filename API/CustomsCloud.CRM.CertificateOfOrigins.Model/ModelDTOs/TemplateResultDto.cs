namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy TemplateResult [DataContract] — a single generated certificate template document. The BL uses Content
// (the file bytes uploaded as the attachment) and DocumentTypeId; the other fields mirror the legacy contract.
public class TemplateResultDto
{
    public int Id { get; set; }

    public string? Name { get; set; }

    // The generated file bytes, uploaded as the attachment content.
    public byte[] Content { get; set; } = [];

    public int DocumentTypeId { get; set; }

    public bool IsPdfFormat { get; set; }
}
