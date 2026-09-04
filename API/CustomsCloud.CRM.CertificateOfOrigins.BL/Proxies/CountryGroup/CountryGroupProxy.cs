using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CountryGroupProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), ICountryGroupProxy
{
    public async Task<bool> IsCountryInCountryGroup(int countryId, int countryGroupId)
    {
        // TODO(blocking): confirm the SystemTables CountryCountryGroup endpoint route (until then the mock is enabled
        // via x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/SystemTables/IsCountryInCountryGroup/{countryId}/{countryGroupId}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }

    public async Task<bool> CountryGroupExists(int countryGroupId)
    {
        // TODO(blocking): confirm the SystemTables CountryGroup endpoint route (until then the mock is enabled via
        // x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/SystemTables/CountryGroupExists/{countryGroupId}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }
}
