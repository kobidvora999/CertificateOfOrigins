using Customs.CertificateOfOrigins.Entities;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.AOP;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    public partial class CertificateOfOriginFilter : IExecutableFilter
    {
        [DisplayName("CertificateOfOriginNum")]
        public string certificateNumber { get; set; }

        [DisplayName("CertificateStatus")]
        [PropertyMetadata(RenderType = typeof(CertificateOfOriginStatusCodeEnum))]
        public int? certificateOfOriginStatusID { get; set; }

        [DisplayName("PassportType")]
        [PropertyMetadata(RenderType = typeof(CertificateOfOriginTypeCodeEnum))]
        public int? certificateOfOriginTypeID { get; set; }

        [DisplayName("CustomsAgent")]
        [PropertyMetadata(RenderType = typeof(CustomsAgentCustomerLOV))]
        public int? customsAgentID { get; set; }

        [DisplayName("CustomsHouse")]
        [OrganizationUnitMetadata]
        public int? customsHouseID { get; set; }

        [DisplayName("DestinationCountry")]
        [PropertyMetadata(RenderType = typeof(Customs.Shared.Entities.Country))]
        public int? destinationCountry { get; set; }

        [DisplayName("ExportDeclarationID")]
        public int? exportDeclarationID { get; set; }

        [DisplayName("ExportDeclarationID")]
        public string exportDeclarationNum { get; set; }

        [DisplayName("Exporter")]
        [PropertyMetadata(RenderType = typeof(CustomerLOV))]
        public int? exporterCustomerID { get; set; }

        [DisplayName("FromIssuingDate")]
        public DateTime? fromIssuingDate { get; set; }

        [DisplayName("ToIssuingDate")]
        public DateTime? toIssuingDate { get; set; }

        [DisplayName("RequestDateFrom")]
        public DateTime? fromRequestDate { get; set; }

        [DisplayName("RequestDateTo")]
        public DateTime? toRequestDate { get; set; }

        [DisplayName("RequestReason")]
        [PropertyMetadata(RenderType = typeof(RequestReasonCodeEnum))]
        public int? requestReasonID { get; set; }

        [DisplayName("VersionNumber")]
        public int? versionNumber { get; set; }

        [DisplayName("ShowLatestVersionOnly")]
        public bool? isLastVersion { get; set; }

        public string FunctionName => "[CRM].[usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter]";

        public ObjectParameter[] GetFunctionParameters()
        {
            var temp = new ObjectParameter[]
                        {
                new ObjectParameter("EntitlementID", GetParameterValue(certificateNumber)),
                new ObjectParameter("CustomerID", GetParameterValue(certificateOfOriginStatusID)),
                new ObjectParameter("UpdateUserID", GetParameterValue(certificateOfOriginTypeID)),
                new ObjectParameter("EntitlementCertificateDetailID", GetParameterValue(customsAgentID)),
                new ObjectParameter("EntitlementCertificateID", GetParameterValue(customsHouseID)),
                new ObjectParameter("IsAttributeCanceled", GetParameterValue(destinationCountry)),
                new ObjectParameter("EntitlementID", GetParameterValue(exportDeclarationID)),
                new ObjectParameter("CustomerID", GetParameterValue(exportDeclarationNum)),
                new ObjectParameter("UpdateUserID", GetParameterValue(exporterCustomerID)),
                new ObjectParameter("EntitlementCertificateDetailID", GetParameterValue(fromIssuingDate)),
                new ObjectParameter("EntitlementCertificateID", GetParameterValue(toIssuingDate)),
                new ObjectParameter("IsAttributeCanceled", GetParameterValue(fromRequestDate)),
                new ObjectParameter("EntitlementID", GetParameterValue(toRequestDate)),
                new ObjectParameter("CustomerID", GetParameterValue(requestReasonID)),
                new ObjectParameter("UpdateUserID", GetParameterValue(versionNumber)),
                new ObjectParameter("EntitlementCertificateDetailID", GetParameterValue(isLastVersion))
                        };
            return temp;
        }

        public object GetParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }

        public void MapOutputParameters(IEnumerable<ObjectParameter> parameters)
        {
        }
    }
}
