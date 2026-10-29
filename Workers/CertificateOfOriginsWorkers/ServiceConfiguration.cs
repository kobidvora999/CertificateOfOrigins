using CertificateOfOrigins.DAL;
using CommonServicesExternal;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.DependencyInjection;
using CustomsCloud.InfrastructureCore.Interfaces.General;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Queue.Migration;
using IssueCertificateOfOrigin.Worker;
using IssueCertificateOfOrigin.Worker.Dal;
using IssueCertificateOfOrigin.Worker.Dto;
using IssueCertificateOfOrigin.Worker.Proxies;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOriginsWorkers;

// The service registrations of every CertificateOfOrigins worker hosted here, one section per worker.
[ExcludeFromCodeCoverage]
internal class ServiceConfiguration : IServicesConfiguration
{
    public void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        // === Shared ===
        services.AddLogging();
        services.AddQueueMigration();
        services.AddWcfInvokerUtil();
        services.AddCustomerUtil();
        services.AddHttpProxy();

        // === IssueCertificateOfOrigin worker ===
        services.AddHostedService<IssueCertificateOfOriginWorker>();
        services.AddCustomsDbContext<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>();
        services.AddScoped<IssueCertificateOfOriginHandler>();
        services.AddScoped<IssueCertificateOfOriginBL>();
        services.AddScoped<IIssueCertificateDal, IssueCertificateDal>();
        services.AddParametersUtil();
        services.AddScoped<ICommonExternalProxy, CommonExternalProxy>();
        services.AddScoped<IDocumentsExternalProxy, DocumentsExternalProxy>();
        services.AddScoped<ICommonCloudProxy, CommonCloudProxy>();
        services.AddWcfProxy<ICommonServicesExternalContract>();
        services.AddWcfProxy<IDocumentsExternalContract>();
    }
}
