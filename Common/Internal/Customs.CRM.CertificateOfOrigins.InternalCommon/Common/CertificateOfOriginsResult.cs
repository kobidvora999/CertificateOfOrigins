using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public partial class CertificateOfOriginResult
    {
        public int ID { get; set; }

        public string CertificateNumber { get; set; }

        public string Name { get; set; }

        public int CustomesAgentID { get; set; }

        public string CustomesAgentTitle { get; set; }

        public string CustomesAgentExternalIdNum { get; set; }

        public int ExporterID { get; set; }

        public string ExporterTitle { get; set; }

        public string ExporterExternalIdNum { get; set; }

        public string ExportDeclarationNumber { get; set; }

        public int VersionNumber { get; set; }
        
        public int OrganizationUnitID { get; set; }
        
        public int RequestReasonCode { get; set; }
        
        public DateTime? IssuingDate { get; set; }
        
        public int? LeadDocumentID { get; set; }
    }
}
