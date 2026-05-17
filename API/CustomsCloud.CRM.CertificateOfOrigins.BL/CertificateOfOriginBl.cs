using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;
public class CertificateOfOriginBl(IServiceProvider serviceProvider, ILookupUtil lookupUtil, IParametersUtil paramUtil) : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
}
