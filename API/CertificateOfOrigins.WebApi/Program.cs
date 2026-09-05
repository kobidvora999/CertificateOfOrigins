using CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.DatabaseMigration;
using CustomsCloud.InfrastructureCore.WebApi;

namespace CertificateOfOrigins.WebApi;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = CloudWebApp.CreateCloudWebAppBuilder()
            .UseBaseType<Program>()
            .SetMicroService(CustomsCloud.InfrastructureCore.CustomsMicroServices.CertificateOfOrigins)
            .AddServiceConfiguration<ServicesConfiguration>();

        // TODO(blocking): .AddValidationMessages<ValidationMessages>() — re-enable when the
        // InfrastructureCore package containing BaseValidationMessages reaches the external feed (see ValidationMessages.cs)
        var app = await CloudWebApp.Build(builder);
        new DatabaseMigrationUtil(app).Handle(typeof(Program).Assembly);
        await app.RunAsync();
    }
}
