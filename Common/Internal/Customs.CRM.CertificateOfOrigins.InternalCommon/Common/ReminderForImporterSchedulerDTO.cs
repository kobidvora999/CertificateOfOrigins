using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public class ReminderForImporterSchedulerDTO
    {
        public int DocumentID { get; set; }
        public int AuthenticationFileID { get; set; }
        public int LeadDocumentID { get; set; }
        public int DocumentTypeID { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentTitle { get; set; }
        public int OrganizationUnitID { get; set; }


    }
}
