namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy SendMessageDTO — the payload sent to the Message-Management microservice. Only the fields this service
// populates are modelled: the related entity, the message type, the parameters (file id + status name), and the
// destinations (the current user).
public class SendMessageDto
{
    public VirtualEntityDto? RelatedEntity { get; set; }

    // EMessageTypes numeric value (e.g. ImportRequestDecision = 11102).
    public int MessageTypeId { get; set; }

    public List<string> MessageParameters { get; set; } = [];

    public List<MessageDestinationDto> MultipleMessageDestinations { get; set; } = [];
}
