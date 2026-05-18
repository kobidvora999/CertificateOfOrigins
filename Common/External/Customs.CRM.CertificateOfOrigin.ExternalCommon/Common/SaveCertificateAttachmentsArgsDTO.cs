using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Common
{
    /// <summary>
    /// AuthenticationRequestDTO
    /// </summary>
    public class SaveCertificateAttachmentsArgsDTO
    {
        [DataMember]
        public IEnumerable<TemplateResult> CertificatesTemplates { get; set; }
        [DataMember]
        public string CertificateNumber { get; set; }
        [DataMember]
        public int CertificateID { get; set; }
        [DataMember]
        public int CertificateRequestReasonCode { get; set; }
        [DataMember]
        public int CertificateTypeID { get; set; }
        [DataMember]
        public string AdditionalInfo { get; set; }
    }
}
