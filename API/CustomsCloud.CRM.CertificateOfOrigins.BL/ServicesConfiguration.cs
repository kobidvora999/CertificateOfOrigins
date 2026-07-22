using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ServicesConfiguration : IServicesConfiguration
{
    public void RegisterServices([NotNull] IConfiguration configuration, [NotNull] IServiceCollection services)
    {
        services.AddCustomsDbContext<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>();
        services.AddDataLayer<ICertificateOfOriginsDal, CertificateOfOriginsDal>();
        services.AddBusinessLayer<CertificateOfOriginsBl>();
        services.AddBusinessLayer<AuthenticationRequestBl>();

        // Platform mock-proxy convention: REAL is the default; a request selects the mock per interface via the
        // x-mock-proxy header. TODO(blocking): verify the real Customers endpoint (CustomersByIds) before ROLLOUT.
        services.AddRestProxy();  // IRestProxy for the real proxies' BaseMicroServiceProxyAdapter
        services.AddHttpProxy();  // IProxyMockUtil + per-request mock selection
        services.AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy>();
    }
}
