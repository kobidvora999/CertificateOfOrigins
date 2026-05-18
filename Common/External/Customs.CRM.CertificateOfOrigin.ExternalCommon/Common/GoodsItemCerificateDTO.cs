using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Common
{
    public class GoodsItemCerificateDTO
    {
        [DataMember]
        public int GoodsItemID { get; set; }
        [DataMember]
        public string CertificateNumber { get; set; }
        [DataMember]
        public int? certificateOfOriginID { get; set; }
    }
}
