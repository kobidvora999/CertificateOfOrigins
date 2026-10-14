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

        // TODO(migration): .AddValidationMessages<ValidationMessages>() — no longer blocked on a package: it is in
        // InfrastructureCore.WebApi 1.10.144 and BaseValidationMessages<T> in Interfaces 1.10.51 (the Customers repo uses the
        // pattern). Still owed: a ValidationMessages class + resx with the legacy UIMessage/ServerTerms texts, and a local
        // code/English-text map for the reconciliation messages (ValidationMessage carries neither). See MIGRATION-NOT-DONE.
        var app = await CloudWebApp.Build(builder);
        new DatabaseMigrationUtil(app).Handle(typeof(Program).Assembly);
        await app.RunAsync();
    }
}
