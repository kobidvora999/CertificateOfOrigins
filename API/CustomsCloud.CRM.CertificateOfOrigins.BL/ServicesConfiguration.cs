using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using CustomsCloud.InfrastructureCore.Lookup.Infrastructure;
using CustomsCloud.InfrastructureCore.Parameters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ServicesConfiguration : IServicesConfiguration
{
    public void RegisterServices([NotNull] IConfiguration configuration, [NotNull] IServiceCollection services)
    {
        services.AddCustomsDbContext<CertificateOfOriginDbContext, CertificateOfOriginDbReadOnlyContext>();
        services.AddDataLayer<ICertificateOfOriginDal, CertificateOfOriginDal>();
        services.AddBusinessLayer<CertificateOfOriginBl>();
        services.AddBusinessLayer<AuthenticationRequestBl>();
        services.AddRestProxy();
        services.AddScoped<ICustomerProxy, CustomerProxy>();
        services.AddScoped<IUserProxy, UserProxy>();
        services.AddScoped<IVendorProxy, VendorProxy>();
        services.AddScoped<ICollateralProxy, CollateralProxy>();
        services.AddScoped<ITasksProxy, TasksProxy>();
        services.AddScoped<IDocumentsProxy, DocumentsProxy>();
        services.AddLookup<Country>();
        services.AddLookup<OrganizationUnit>();
        services.AddParametersService();
    }
}
