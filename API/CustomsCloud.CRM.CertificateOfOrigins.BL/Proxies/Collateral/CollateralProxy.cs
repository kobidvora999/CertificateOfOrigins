using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CollateralProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Collaterals), ICollateralProxy
{
    // Legacy: Container.Resolve<ICollateralExternalProxy>().GetCollateralRequest(EEntityType.ImportAuthenticationRequest,
    // entityId, null) — the collaterals attached to an entity live in the Collateral microservice.
    public async Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"Collateral/CollateralRequestByEntity/{entityType}/{entityId}"); // TODO(blocking): confirm endpoint name/route with the Collateral microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CollateralRequestDto>>();
    }
}
