using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Queue.Migration;
using CustomsCloud.InfrastructureCore.Worker.CloudApp;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOriginsWorkers;

// Runs every CertificateOfOrigins worker (see ServiceConfiguration) and migrates the queues they consume.
[ExcludeFromCodeCoverage]
public class Program
{
    protected Program()
    {
    }

    public static async Task Main()
    {
        var builder = CloudWorkerApp.CreateCloudWorkerAppBuilder()
           .UseBaseType<Program>()
           .SetMicroService(CustomsMicroServices.CertificateOfOrigins)
           .AddServiceConfiguration<ServiceConfiguration>();

        var app = await CloudWorkerApp.Build(builder);
        await Startup(app);
        await app.RunAsync();
    }

    public static async Task Startup(IHost app)
    {
        await ExecuteQueueMigration(app.Services);
    }

    private static async Task ExecuteQueueMigration(IServiceProvider serviceProvider)
    {
        var util = serviceProvider.GetRequiredService<IQueueMigration>();

        await util.MigrateOneWay();

        var builder = util.CreateBuilder();

        // === IssueCertificateOfOrigin ===
        // The API publishes a certificate to issue to this exchange, named by CertificateOfOriginsConsts.IssueCertificateOfOriginExchange,
        // a failed message is dead-lettered to the Errors queue.
        builder
            .WithQueue(b => b.WithName("IssueCertificateOfOrigin")
                .WithDeadLetterExchange("IssueCertificateOfOrigin")
                .WithDeadLetterExchangeRoutingKey("Error")
                .WithMaxLengthBytes(250_000_000))
            .WithQueue(b => b.WithName("IssueCertificateOfOrigin.Errors")
                .WithMaxLengthBytes(250_000_000));

        builder
            .WithExchange(b => b.WithName("IssueCertificateOfOrigin").SetDirectType());

        builder
            .WithBinding(b => b.WithExchange("IssueCertificateOfOrigin").WithQueue("IssueCertificateOfOrigin").WithRoutingKey(string.Empty))
            .WithBinding(b => b.WithExchange("IssueCertificateOfOrigin").WithQueue("IssueCertificateOfOrigin.Errors").WithRoutingKey("Error"));

        var migration = builder.Build();
        await util.Migrate(migration);
    }
}
