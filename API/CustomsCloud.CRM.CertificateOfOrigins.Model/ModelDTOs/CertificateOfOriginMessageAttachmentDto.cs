namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// One rendered attachment returned with the certificate feedback (legacy EAISchema Attachment — only the fields the
// legacy CreateAttachments populated: DocumentTypeID + content + fileName).
public class CertificateOfOriginMessageAttachmentDto
{
    public int DocumentTypeId { get; set; }

    public byte[]? Content { get; set; }

    public string? FileName { get; set; }
}
