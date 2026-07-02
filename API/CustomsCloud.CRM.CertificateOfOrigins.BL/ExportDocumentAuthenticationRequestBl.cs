using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ExportDocumentAuthenticationRequestBl(IServiceProvider serviceProvider, ICustomerProxy customerProxy)
    : BaseBL<ExportDocumentAuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
}
