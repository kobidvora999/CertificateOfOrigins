using CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.PlanarJob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Planar.Job;

namespace CertificateOfOrigins.Planar.ReminderForImporterScheduler.Jobs;

// Migrated from the legacy BaseScheduleTask ReminderForImporterScheduler.
//
// Opens a "send reminder to importer" task for every import-authentication request whose importer letter has gone
// unanswered past the reminder window. The BL decides which requests are due (including the "no such task already
// open" check, which used to live in the SP); this job only drives the loop and reports the count.
public class ReminderForImporterSchedulerJob : BaseJob
{
    public override void Configure(IConfigurationBuilder configurationBuilder, IJobExecutionContext context)
        => configurationBuilder.AddCustomsCoreConfiguration();

    public override void RegisterServices(IConfiguration configuration, IServiceCollection services, IJobExecutionContext context)
    {
        // Load-bearing: registers ICustomsApplicationInfo, EnvironmentFactory, IMockUtil, the heartbeat and the
        // RabbitMQ + Redis connections. Without it RaiseEvent compiles but fails at run time with no broker.
        services.AddCustomsCoreServices(context);

        // Neither AddCustomsCoreServices nor the BL's own registration adds IMapper (the WebApi host normally
        // does), and BaseBL.Mapper resolves it — so the first map would throw without this.
        services.AddAutoMapperProfiles<AuthenticationRequestBl>();

        // Reuse the BL registration so the DbContext → DAL → BL → proxies → utils chain is wired exactly as the
        // WebApi wires it, rather than re-registering pieces by hand and drifting.
        new CertificateOfOrigins.BL.ServicesConfiguration().RegisterServices(configuration, services);
    }

    public override async Task ExecuteJob(IJobExecutionContext context)
    {
        // Planar's ServiceProvider is the root provider while the BL and DbContext are scoped; resolving straight
        // off it would leak a root-scoped DbContext for the life of the job.
        using var scope = ServiceProvider.CreateScope();
        var bl = scope.ServiceProvider.GetRequiredService<AuthenticationRequestBl>();

        var due = await bl.GetImportAuthenticationRequestsForReminderForImporterScheduler();
        Logger.LogInformation("{Count} import authentication request(s) are due an importer reminder", due.Count);

        var raised = 0;
        foreach (var request in due)
        {
            try
            {
                await bl.RaiseEventForReminderForImporterScheduler(request);
                raised++;
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException or TimeoutException)
            {
                // Legacy behaviour was to let the batch run to completion, so one unreachable broker or one bad row
                // must not cost the rest of the night's reminders. The filter narrows the legacy catch-everything:
                // an exception type outside this list WILL now abort the batch, which is the deliberate trade for
                // staying inside Sonar S2221.
                Logger.LogError(ex, "raising the importer reminder for document {DocumentId} failed; continuing", request.DocumentId);
            }
        }

        await SetEffectedRowsAsync(raised);
    }
}
