using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Users;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICollateralProxy collateralProxy,
    ITasksProxy tasksProxy,
    ILookupUtil lookupUtil,
    IParametersUtil parametersUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
}
