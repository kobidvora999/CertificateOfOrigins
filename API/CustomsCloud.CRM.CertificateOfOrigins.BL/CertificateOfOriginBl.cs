using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginBl(IServiceProvider serviceProvider)
    : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
}
