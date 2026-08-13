namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Incoming (portal/EAI) WCF: GetPC_MSG2280_2281_CertificateOfOriginRequest (PC_NG_2280_MSG01) — a certificate-of-origin
// request submitted by an agent. The legacy one-way WCF message (callback/MSMQ response) is exposed here as a
// synchronous REST POST that returns the feedback directly (developer decision — mirrors the migrated sibling
// GetCertificateRequestByGuid and the legacy *Sync contract). XML-serialization *Specified companion flags are dropped —
// a nullable value being null carries the same "not supplied" meaning.
public class CertificateOfOriginRequestMessageDto
{
    // The submitting agent's customer id (legacy CommandRequest.CustomerID from the message header).
    public int CustomerId { get; set; }

    public CertificateOfOriginAgentRequestDto AgentRequest { get; set; } = new();

    // Null for a pure NonManipulation certificate (the certificate body is carried on NonManipulationCertificate).
    public CertificateOfOriginMessageDto? CertificateOfOrigin { get; set; }

    public NonManipulationCertificateMessageDto? NonManipulationCertificate { get; set; }
}
