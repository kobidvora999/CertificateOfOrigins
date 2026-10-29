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
            .AddServiceConfiguration<ServicesConfiguration>()
            .AddValidationMessages<ValidationMessages>();

        var app = await CloudWebApp.Build(builder);
        ////new DatabaseMigrationUtil(app).Handle(typeof(Program).Assembly);
        await app.RunAsync();
    }
}
