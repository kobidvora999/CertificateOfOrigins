using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;
using Customs.Inf.CommonService.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class CommonServicesServiceAdapter : BaseServiceAdapter<ICommonServicesExternalProxy>, ICommonServicesServiceAdapter
    {
        public TemplateResult GanerateReportAndConvertToTemplateResult(int reportID, int certificateOfOriginId, string additionalInfo)
        {
            var reportResult = ExternalProxy.GenerateReportByParams(reportID, new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("EntityID", certificateOfOriginId.ToString()),
                    new KeyValuePair<string, string>("AdditionalInfo", additionalInfo),
                }, "PDF");

            return new TemplateResult() { Content = reportResult, IsPdfFormat=true, DocumentTypeID = 329 };


        }
      

        public TemplateResult CreatePDFTemplateSync(int certificateOfOriginId, int templateTypeId, string additionalInfo)
        {
            var filter = new TemplateDataFilter
            {
                EntityID = certificateOfOriginId,
                TemplateTypeID = templateTypeId, 
                AdditionalInfo = additionalInfo
            };

            var templateResult = ExternalProxy.CreatePDFTemplateSync(filter);

            return templateResult;
        }

        public byte[] CreateQRCode(string url)
        {
            return ExternalProxy.CreateQRCode(url);
        }
    }
}
