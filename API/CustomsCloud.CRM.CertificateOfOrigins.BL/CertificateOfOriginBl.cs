using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class CertificateOfOriginBl(IServiceProvider serviceProvider)
    : BaseBL<CertificateOfOriginBl, ICertificateOfOriginDal>(serviceProvider)
{
}
