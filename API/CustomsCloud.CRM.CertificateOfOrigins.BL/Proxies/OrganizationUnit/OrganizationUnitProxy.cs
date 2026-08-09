using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class OrganizationUnitProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), IOrganizationUnitProxy
{
    public async Task<bool> IsOrganizationUnitCustomsHouse(int organizationUnitId)
    {
        // TODO(blocking): confirm the owning microservice (org-unit / SystemTables) + endpoint route
        // (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"OrganizationUnit/IsCustomsHouse/{organizationUnitId}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }
}
