using CustomsCloud.InfrastructureCore.Queue;
using CustomsCloud.InfrastructureCore.Worker;
using System.Diagnostics.CodeAnalysis;

namespace IssueCertificateOfOrigin.Worker;

public class IssueCertificateOfOriginWorker : BaseWorker<IssueCertificateOfOriginWorker>
{
    [ExcludeFromCodeCoverage]
    public IssueCertificateOfOriginWorker(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeQueue();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fail to execute worker {Name}", nameof(IssueCertificateOfOriginWorker));
        }
    }

    protected override async Task HealthCheck()
    {
        await IsQueueHealthy();
    }

    protected override async Task HandleMessageAsync(MessageEventArgs args)
    {
        using var scope = CreateProviderScope(args);
        using var logScope = CreateLoggerScope(args, nameof(IssueCertificateOfOriginWorker));

        try
        {
            var message = GetCloudEvent(args);
            var handler = scope.ServiceProvider.GetRequiredService<IssueCertificateOfOriginHandler>();
            await handler.Handle(message);
            await args.Acknowledge();
        }
        catch (Exception ex)
        {
            await SafeHandleWorkerException(ex, args, scope);
        }
    }
}
