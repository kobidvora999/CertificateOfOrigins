namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The AgentRequest header of the incoming certificate-of-origin message (PC_NG_2280_MSG01 AgentRequest) — the
// operation-level fields: which certificate type, why (request reason), and the certificate ids the reason refers to.
public class CertificateOfOriginAgentRequestDto
{
    public string? InternalApplication { get; set; }

    public int CertificateOfOriginTypeCode { get; set; }

    public int RequestReasonCode { get; set; }

    // The certificate number this request targets (existing certificate). Empty for a brand-new certificate.
    public string? CertificateId { get; set; }

    public string? CertificateIdToCancel { get; set; }

    public string? ReplacementReason { get; set; }

    public string? ExportDeclarationNum { get; set; }
}
