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
            .WithResource($"SystemTables/IsCountryInCountryGroup/{countryId}/{countryGroupId}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }
}
