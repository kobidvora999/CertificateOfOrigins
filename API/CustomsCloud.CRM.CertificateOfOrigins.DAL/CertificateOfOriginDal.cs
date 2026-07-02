using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public class CertificateOfOriginDal(IServiceProvider serviceProvider)
    : BaseDal<CertificateOfOriginDbContext, CertificateOfOriginDbReadOnlyContext>(serviceProvider), ICertificateOfOriginDal
{
}
