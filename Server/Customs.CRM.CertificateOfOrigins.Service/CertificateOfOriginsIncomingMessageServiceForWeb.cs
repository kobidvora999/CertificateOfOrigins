using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using Customs.Inf.SystemTables.EAISchema;
using Customs.Inf.WebMultiDotNetSupport.CertificateOfOrigins;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Service
{
    public partial class CertificateOfOriginsIncomingMessageService
    {
        public WebInternalServiceOutResponse InternalGetCertificateRequestByGuid(WebInternalServiceInRequest request)
        {
            var requestData = JSONSerializeHelper<CertificateOfOriginsRequest>.JSONDeserialize(request.Content.Content);
            var certificateOfOriginsResponse = new CertificateOfOriginsResponse();
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var certificateOfOriginWebBL = new CertificateOfOriginWebBL(uow);
                certificateOfOriginsResponse = certificateOfOriginWebBL.GetCertificateDetailForWeb(requestData);
            }

            var response = new WebInternalServiceOutResponse
            {
                Content = { Content = JSONSerializeHelper<CertificateOfOriginsResponse>.JSONSerialize(certificateOfOriginsResponse) }
            };
            return response;
        }
    }
}
