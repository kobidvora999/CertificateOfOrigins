using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class OrganizationUnitMockProxy(IProxyMockUtil mockUtil) : IOrganizationUnitProxy, IMockProxy
{
    // Default = the org unit IS a customs house (validation passes); feature "OrgUnit.NotCustomsHouse" flips it.
    public Task<bool> IsOrganizationUnitCustomsHouse(int organizationUnitId)
    {
        return Task.FromResult(!mockUtil.HasMockFeature("OrgUnit.NotCustomsHouse"));
    }
}
