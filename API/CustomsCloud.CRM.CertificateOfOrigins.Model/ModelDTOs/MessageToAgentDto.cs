namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Talkback message to the customs agent — sent via the Common service.
// TODO(blocking): verify the contract (field names/casing) against the Common service's endpoint.
public class MessageToAgentDto
{
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }

    public EAgentTalkBackType AgentTalkBackType { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsExport { get; set; }
}
