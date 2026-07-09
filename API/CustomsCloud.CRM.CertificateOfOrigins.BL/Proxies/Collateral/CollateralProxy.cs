using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CollateralProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Collaterals), ICollateralProxy
{
    public async Task<List<CollateralRequestDto>?> GetCollateralRequest(int entityType, int entityId, int? collateralRequestId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Collateral/CollateralRequest") // TODO: confirm endpoint name (route = target BL name) with the Collaterals microservice
            .AddQueryStringParameter("entityType", entityType)
            .AddQueryStringParameter("entityId", entityId)
            .AddQueryStringParameter("collateralRequestId", collateralRequestId);
        return (await ExecuteAsync<List<CollateralRequestDto>>(req)).Data;
    }
}
