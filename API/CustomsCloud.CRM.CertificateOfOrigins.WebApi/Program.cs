using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.DatabaseMigration;
using CustomsCloud.InfrastructureCore.WebApi;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi;

public class Program
{
    protected Program()
    {
    }

    public static void Main(string[] args)
    {
        var builder = CloudWebApp.CreateCloudWebAppBuilder()
            .UseBaseType<Program>()
            .SetMicroService(InfrastructureCore.CustomsMicroServices.CertificateOfOrigins)
            .AddServiceConfiguration<ServicesConfiguration>();

        var app = CloudWebApp.Build(builder);
        DatabaseMigrationUtil.Handle(app, "CertificateOfOrigins");
        app.Run();
    }
}
