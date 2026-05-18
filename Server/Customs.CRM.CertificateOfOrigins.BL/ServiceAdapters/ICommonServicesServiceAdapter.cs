using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Customs.Inf.CommonService.ExternalCommon.TemplateDTOs;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface ICommonServicesServiceAdapter
    {
        TemplateResult GanerateReportAndConvertToTemplateResult(int reportID ,int certificateOfOriginId,string additionalInfo);
        TemplateResult CreatePDFTemplateSync(int certificateOfOriginId, int templateTypeId, string additionalInfo);

        byte[] CreateQRCode(string url);
    }
}
