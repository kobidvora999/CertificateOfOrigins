namespace CertificateOfOrigins.Model.ModelDTOs;

// One certificate document returned with the certificate feedback, by id only (V2 / pure REST attachment): the
// community reads the content from the Documents service. Legacy returned the rendered bytes in the message.
public class CertificateOfOriginMessageAttachmentDto
{
    public string ExternalAttachmentId { get; set; } = null!;
}
