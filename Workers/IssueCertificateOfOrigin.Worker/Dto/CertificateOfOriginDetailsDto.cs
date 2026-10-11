using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IssueCertificateOfOrigin.Worker.Dto
{
    public class CertificateOfOriginDetailsDto
    {
        public string? CertificateNumber { get; set; }
        public int RequestReasonCode { get; set; }
        public int CertificateTypeID { get; set; }
        public int StatusId { get; set; }
        public bool IsInPublishingProcess { get; set; }
    }
}
