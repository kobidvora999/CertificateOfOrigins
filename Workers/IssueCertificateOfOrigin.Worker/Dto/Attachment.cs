using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace IssueCertificateOfOrigin.Worker.Dto
{
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [GeneratedCode("System.Xml", "4.8.4084.0")]
    [XmlRoot(Namespace = "http://malam.com/customs/EAICommon.xsd", IsNullable = true)]
    [XmlType(Namespace = "http://malam.com/customs/EAICommon.xsd")]
    public class Attachment
    {
        [XmlElement(DataType = "base64Binary", IsNullable = true, Order = 0)]
        public required byte[] content;
        [XmlElement(DataType = "token", IsNullable = true, Order = 7)]
        public string? Remark;
        [XmlElement("AdditionalData", Order = 6)]
        public AttachmentAdditionalData[]? AdditionalData;
        [XmlElement(IsNullable = true, Order = 5)]
        public string? externalAttachmentID;
        [XmlAttribute(DataType = "token")]
        public string? IsAttachment;
        [XmlElement(DataType = "token", IsNullable = true, Order = 3)]
        public string? keywords;
        [XmlElement(DataType = "token", IsNullable = true, Order = 2)]
        public string? documentType;
        [XmlElement(DataType = "token", IsNullable = true, Order = 1)]
        public string? fileName;
        [XmlElement(IsNullable = true, Order = 4)]
        public string? attachmentID;

        [XmlIgnore]
        public bool? IsValidForNewEntityCreation { get; set; }
        [XmlIgnore]
        public int DocumentTypeID { get; set; }
        [XmlIgnore]
        public string? DocumentTypeName { get; set; }
        [XmlIgnore]
        public bool IsDocuemtAccepted { get; set; }
        public bool IsFileWillBeSendLater { get; }
        public bool IsAlreadyInMalam { get; }
        [XmlIgnore]
        public bool? IsRequired { get; set; }
        [XmlIgnore]
        public string? Title { get; set; }
    }
}
