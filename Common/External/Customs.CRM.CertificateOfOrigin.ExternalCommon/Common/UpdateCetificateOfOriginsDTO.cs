using Customs.CRP.External.DealFile.Entities;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Common
{
    [DataContract]
    public class UpdateCetificateOfOriginsDTO
    {
        [DataMember]
        public EEventType EventType { get; set; }
        [DataMember]
        public int LeadDocumentID { get; set; }
        [DataMember]
        public string ExportDeclarationNum { get; set; }
        [DataMember]
        public List<int> CertificateOfOriginsIDs { get; set; }
        [DataMember]
        public int? ExporterCustomerID { get; set; }
        [DataMember]
        public int? DestinationCoutryID { get; set; }
        [DataMember]
        public int OrganizationUnitID { get; set; }
        [DataMember]
        public List<ExportInvoiceInfoDTO> ExportInvoiceInfoDTOList { get; set; }

    }

}
