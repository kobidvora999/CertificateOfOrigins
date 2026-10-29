using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Interfaces.General;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): local stand-in for the platform's BaseLookupResolver<T>, which is internal to
// CustomsCloud.InfrastructureCore.Lookup and serves only the types declared there. A lookup type the platform does not
// have yet is declared in this folder and loaded by this resolver, the same way the platform loads its own: GET
// lookup/{Type} from the source service, kept in the distributed (Redis) cache, refreshed every 360 minutes. Registered
// under the type key, so lookupUtil.Get<T> / Search<T> resolve it exactly like a platform lookup. Locally the source is
// served by tools/local-lookup-stub.js. In the internal environment, once the platform package has the type: delete the
// local record, register services.AddLookup<Lookup.T>() instead of AddLocalLookup, and the BL calls stay as they are.
// The source service comes in as LocalLookupSource<T> so DI can build the resolver anywhere — the heartbeat registration
// activates it by type.
public class LocalLookupResolver<T>(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory, LocalLookupSource<T> source)
    : BaseDistributedResolver<int, T>(serviceProvider, 360, 10)
    where T : ILookup
{
    protected override async Task<Dictionary<int, T>> LoadDataAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var httpProxy = scope.ServiceProvider.GetRequiredService<IHttpProxy>();
        var request = httpProxy.CreateRequestBuilder()
            .UseGetMethod()
            .ToCustomsService(source.Service)
            .WithResource("lookup/" + typeof(T).Name)
            .WithCancellationToken(cancellationToken)
            .Build();
        var response = await httpProxy.ExecuteAsync(request);
        response.Validate();
        var result = await response.GetResult<IEnumerable<T>>() ?? [];
        return result.ToDictionary(item => item.Id);
    }
}

// The source service a local lookup type loads from (GET lookup/{Type}), registered per type by AddLocalLookup.
// T is only the DI discriminator: one registration per lookup type, so each resolver gets its own source.
#pragma warning disable S2326 // T is the DI key, not data
public sealed class LocalLookupSource<T>(CustomsMicroServices service)
#pragma warning restore S2326
    where T : ILookup
{
    public CustomsMicroServices Service { get; } = service;
}
