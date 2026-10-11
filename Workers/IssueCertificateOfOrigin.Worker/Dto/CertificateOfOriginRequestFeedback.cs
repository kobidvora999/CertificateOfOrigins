using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IssueCertificateOfOrigin.Worker.Dto
{
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [GeneratedCode("System.Xml", "4.0.30319.18408")]
    [XmlRoot(Namespace = "http://malam.com/customs/EAICommon.xsd", IsNullable = true)]
    [XmlType(Namespace = "http://malam.com/customs/EAICommon.xsd")]
    public class CertificateOfOriginRequestFeedback
    {
        [XmlElement(IsNullable = true, DataType = "token", Order = 0)]
        public string? InternalApplication { get; set; }

        [XmlElement(IsNullable = true, DataType = "token", Order = 1)]
        public string? CertificateID { get; set; }

        [XmlElement(Order = 2)]
        public int CertificateOfOriginTypeCode { get; set; }

        [XmlElement(Order = 3)]
        public int CertificateOfOriginStatusCode { get; set; }

        [XmlElement(IsNullable = true, DataType = "token", Order = 4)]
        public string? FeedbackRemark { get; set; }

        [XmlElement(IsNullable = true, DataType = "token", Order = 5)]
        public string? RejectCancelReason { get; set; }

        [XmlElement(IsNullable = true, DataType = "token", Order = 6)]
        public string? QueryURL { get; set; }

        [XmlElement(Order = 7)]
        public int RequestReasonCode { get; set; }
    }
}
