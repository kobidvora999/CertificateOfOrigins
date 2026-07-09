using CustomsCloud.InfrastructureCore.Utils.OutgoingMessage;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.OutgoingMessages;

// Outgoing EAI message PC_NG_2281_MSG02 — certificate-of-origin request feedback to the customer.
// Lives in the BL (not Model): it implements the infra IOutgoingMessage for AddMessage<T>.
// ⚠️ External message contract: field names/casing preserved EXACTLY from the legacy generated classes
//    (PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback*) — do NOT rename to .NET conventions.
#pragma warning disable IDE1006, SA1300, SA1402 // legacy contract casing; contract classes kept together
public class CertificateOfOriginRequestFeedbackMessage : IOutgoingMessage
{
    // TODO(blocking): SendService routing value — legacy routed by the PC_NG_2281_MSG02 generated type;
    // confirm the correct SendService member with the OutgoingMessages infra before internal integration
    public SendService SendService => default;

    public CertificateOfOriginRequestFeedback? CertificateOfOriginRequestFeedback { get; set; }

    public CertificateAttachment[]? Attachment { get; set; }
}

public class CertificateOfOriginRequestFeedback
{
    public string? rejectCancelReason { get; set; }

    public string? internalApplication { get; set; }

    public int certificateOfOriginTypeCode { get; set; }

    public int certificateOfOriginStatusCode { get; set; }

    public string? certificateID { get; set; }

    public string? FeedbackRemark { get; set; }

    public string? QueryURL { get; set; }

    public int requestReasonCode { get; set; }

    public DateTime? IssueDateIfReleased { get; set; }

    public bool IssueDateIfReleasedSpecified { get; set; }

    public DateTime? IssueDateIfNotReleased { get; set; }

    public bool IssueDateIfNotReleasedSpecified { get; set; }
}

public class CertificateAttachment
{
    public int DocumentTypeID { get; set; }

    public byte[]? content { get; set; }

    public string? fileName { get; set; }
}
#pragma warning restore IDE1006, SA1300, SA1402
