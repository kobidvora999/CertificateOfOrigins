using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class OrganizationUnitProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Users), IOrganizationUnitProxy
{
    public async Task<bool> IsOrganizationUnitCustomsHouse(int organizationUnitId)
    {
        // Legacy UserServiceAdapter → Users ExternalProxy.IsOrganzationUnitCustomHouse: the Users service answers from the org
        // unit's TYPE (GetOrganizationUnitWithTypeByID → OrganizationUnitType.IsCustomsHouse; unknown unit → false). Kept as a
        // call to the owning service: the Lookup.OrganizationUnit.IsCustomsHouse flag is not the same rule.
        // TODO(blocking): confirm the Users endpoint route (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("api/OrganizationUnit/IsCustomsHouse/{organizationUnitId}")
            .AddUrlSegmentParameter("organizationUnitId", organizationUnitId);
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }
}
