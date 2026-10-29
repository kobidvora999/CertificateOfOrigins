namespace CertificateOfOrigins.Model.ModelDTOs;

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

    // Legacy TemplateResult.FileName — a computed property, not a stored one: Name + the extension implied by
    // IsPdfFormat. It is what CreateAttachments put on the feedback's attachment, so it is reproduced here rather
    // than reinvented at the call site.
    public string FileName => IsPdfFormat ? $"{Name}.pdf" : $"{Name}.docx";
}
