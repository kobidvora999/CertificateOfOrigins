using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CountryGroupMockProxy(IProxyMockUtil mockUtil) : ICountryGroupProxy, IMockProxy
{
    // Default = the country IS in the country group (the agreement check passes); feature
    // "CountryGroup.NotInGroup" flips it so the country-group discrepancy exception path is exercised.
    public Task<bool> IsCountryInCountryGroup(int countryId, int countryGroupId)
    {
        return Task.FromResult(!mockUtil.HasMockFeature("CountryGroup.NotInGroup"));
    }

    // Default = the country group exists (validation passes); feature "CountryGroup.NotExists" flips it so the
    // "value not in system" path is exercised.
    public Task<bool> CountryGroupExists(int countryGroupId)
    {
        return Task.FromResult(!mockUtil.HasMockFeature("CountryGroup.NotExists"));
    }
}
