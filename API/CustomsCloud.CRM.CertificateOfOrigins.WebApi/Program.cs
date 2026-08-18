using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.DatabaseMigration;
using CustomsCloud.InfrastructureCore.WebApi;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi;

public class Program
{
    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = CloudWebApp.CreateCloudWebAppBuilder()
            .UseBaseType<Program>()
            .SetMicroService(InfrastructureCore.CustomsMicroServices.CertificateOfOrigins)
            .AddServiceConfiguration<ServicesConfiguration>();

        // TODO(blocking): .AddValidationMessages<ValidationMessages>() — re-enable when the
        // InfrastructureCore package containing BaseValidationMessages reaches the external feed (see ValidationMessages.cs)
        var app = await CloudWebApp.Build(builder);
        new DatabaseMigrationUtil(app).Handle(typeof(Program).Assembly);
        await app.RunAsync();
    }
}
