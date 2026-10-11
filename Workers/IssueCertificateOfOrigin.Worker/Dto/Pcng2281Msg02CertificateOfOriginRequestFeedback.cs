using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace IssueCertificateOfOrigin.Worker.Dto
{
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [GeneratedCode("System.Xml", "4.0.30319.18408")]
    [XmlRoot(Namespace = "http://malam.com/customs/CertificateOfOrigins/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback", IsNullable = false)]
    [XmlType(AnonymousType = true, Namespace = "http://malam.com/customs/CertificateOfOrigins/PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback")]
#pragma warning disable S101
    public class PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback
#pragma warning restore S101
    {
        // The outgoing-message service name. It was SendService.SendPC_NG_2281_MSG02_CertificateOfOriginRequestFeedback
        // (IOutgoingMessage, InfrastructureCore.Utils.OutgoingMessage), which Utils removed after 1.10.99; the value is the
        // same enum name, and the message is sent through ICommonCloudProxy, not the Utils outgoing-message service.
        public const string SendServiceName = "SendPC_NG_2281_MSG02_CertificateOfOriginRequestFeedback";

        [XmlElement(Order = 0)]
        public CertificateOfOriginRequestFeedback? CertificateOfOriginRequestFeedback { get; set; }

        [XmlElement(IsNullable = true, Order = 1)]
        public Attachment? Attachment { get; set; }
    }
}
