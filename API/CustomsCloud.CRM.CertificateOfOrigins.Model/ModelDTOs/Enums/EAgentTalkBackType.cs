using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Values verified against legacy Customs.Infrastructure.DocumentManagement.ExternalCommon\Enums\EAgentTalkBackType.cs
public enum EAgentTalkBackType
{
    [Display(Name = "Certificate of origin cancellation")]
    [Description("CertificateOfOriginCancellation")]
    CertificateOfOriginCancellation = 38,

    [Display(Name = "Certificate of origin replacement")]
    [Description("CertificateOfOriginReplacement")]
    CertificateOfOriginReplacement = 39
}
