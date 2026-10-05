using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces;
using CustomsCloud.InfrastructureCore.Interfaces.General;
using CustomsCloud.InfrastructureCore.Lookup;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): the registration twin of the platform's AddLookup<T> for the local lookup types in this folder (same
// singleton + IResolver + type-keyed IResolver + heartbeat registrations). Replace each call with services.AddLookup<T>()
// once the platform package has the type. AddLookup of any platform type must run first (it registers ILookupUtil).
public static class LocalLookupServicesConfiguration
{
    public static IServiceCollection AddLocalLookup<T>(this IServiceCollection services, CustomsMicroServices sourceService)
        where T : ILookup
    {
        services.AddSingleton(provider => new LocalLookupResolver<T>(provider, provider.GetRequiredService<IServiceScopeFactory>(), sourceService));
        services.AddSingleton<IResolver>(provider => provider.GetRequiredService<LocalLookupResolver<T>>());
        services.AddKeyedSingleton<IResolver>(typeof(T), (provider, _) => provider.GetRequiredService<LocalLookupResolver<T>>());
        services.AddHeartBeatSubscriber<LocalLookupResolver<T>>(ServiceLifetime.Transient);
        return services;
    }
}
