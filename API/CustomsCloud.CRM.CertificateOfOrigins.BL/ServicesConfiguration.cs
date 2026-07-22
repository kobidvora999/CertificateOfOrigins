using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
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
        services.AddRestProxy();

        // TODO(blocking): switch to the real CustomerProxy via the C4 mock-proxy pattern (AddProxy<I,Real,Mock>
        // + x-mock-proxy header + ProxyMocking shim) once the Customers endpoint is verified. Mock registered
        // directly for now so the service runs locally without the Customers microservice.
        services.AddScoped<ICustomerProxy, CustomerMockProxy>();
    }
}
