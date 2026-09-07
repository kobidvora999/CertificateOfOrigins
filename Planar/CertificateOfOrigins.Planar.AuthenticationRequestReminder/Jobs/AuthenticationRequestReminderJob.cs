using CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.PlanarJob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Planar.Job;

namespace CertificateOfOrigins.Planar.AuthenticationRequestReminder.Jobs;

// Migrated from the legacy BaseScheduleTask AuthenticationRequestReminder (file RequestReminder.cs).
//
// Walks the reminder ladder: every authentication-request file or export request that has reached a reminder window
// gets the matching task opened. Which rung fires is decided in the BL from the delivery method plus the
// import/vendor/three-month flags, so this job stays a shell over one read and one batch call — same shape as legacy.
public class AuthenticationRequestReminderJob : BaseJob
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

        var due = await bl.GetAuthenticationRequestsForScheduler();
        if (due.Count == 0)
        {
            // The legacy task returned 0 early on an empty set; keep that so a quiet night reads as a quiet night
            // rather than as a failure.
            Logger.LogInformation("no authentication requests are due a reminder");
            await SetEffectedRowsAsync(0);
            return;
        }

        Logger.LogInformation("{Count} authentication request(s) reached a reminder window", due.Count);

        // The per-row skip (a task of that type is already open) happens inside RaiseSchedulerEvents, so the count
        // that comes back is tasks actually opened, not rows examined — same as the legacy return value.
        var opened = await bl.RaiseSchedulerEvents(due);
        Logger.LogInformation("{Opened} reminder task(s) opened out of {Candidates} candidate(s)", opened, due.Count);

        await SetEffectedRowsAsync(opened);
    }
}
