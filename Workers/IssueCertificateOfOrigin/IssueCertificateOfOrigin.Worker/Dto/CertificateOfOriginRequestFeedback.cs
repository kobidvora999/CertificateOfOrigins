using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace IssueCertificateOfOrigin.Worker.Dto
{
    // The CertificateOfOriginRequestFeedback element of message PC_NG_2281_MSG02, exactly as the legacy EAI schema class
    // (Customs.Inf.CertificateOfOrigins.EAISchema PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackCertificateOfOriginRequestFeedback):
    // the message's own namespace and the schema's element names — camelCase except FeedbackRemark, QueryURL and the two
    // IssueDate elements. XmlSerializer is case- and namespace-sensitive, so any other name is an unknown element to the
    // agent's system (parity finding GN-01).
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [GeneratedCode("System.Xml", "4.8.3752.0")]
    [XmlType(AnonymousType = true, Namespace = "http://malam.com/customs/CertificateOfOrigins/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback")]
    public class CertificateOfOriginRequestFeedback
    {
        [XmlElement("internalApplication", DataType = "token", Order = 0)]
        public string? InternalApplication { get; set; }

        [XmlElement("certificateID", DataType = "token", Order = 1)]
        public string? CertificateID { get; set; }

        [XmlElement("certificateOfOriginTypeCode", Order = 2)]
        public int CertificateOfOriginTypeCode { get; set; }

        [XmlElement("certificateOfOriginStatusCode", Order = 3)]
        public int CertificateOfOriginStatusCode { get; set; }

        [XmlElement("FeedbackRemark", IsNullable = true, DataType = "token", Order = 4)]
        public string? FeedbackRemark { get; set; }

        [XmlElement("rejectCancelReason", IsNullable = true, DataType = "token", Order = 5)]
        public string? RejectCancelReason { get; set; }

        [XmlElement("QueryURL", IsNullable = true, DataType = "token", Order = 6)]
        public string? QueryURL { get; set; }

        [XmlElement("requestReasonCode", Order = 7)]
        public int RequestReasonCode { get; set; }

        // Legacy CertificateOfOriginsUtil.CreateCertificateOfOriginRequestFeedback: the issuing date goes in one of the two
        // elements by whether the declaration was released, and only when both values are known (parity finding GN-02).
        [XmlElement("IssueDateIfReleased", DataType = "date", IsNullable = true, Order = 8)]
        public DateTime? IssueDateIfReleased { get; set; }

        [XmlIgnore]
        public bool IssueDateIfReleasedSpecified { get; set; }

        [XmlElement("IssueDateIfNotReleased", DataType = "date", IsNullable = true, Order = 9)]
        public DateTime? IssueDateIfNotReleased { get; set; }

        [XmlIgnore]
        public bool IssueDateIfNotReleasedSpecified { get; set; }
    }
}
