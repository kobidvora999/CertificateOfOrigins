using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

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
            .WithResource("api/Collateral/CollateralRequestByEntity/{entityType}/{entityId}") // TODO(blocking): confirm endpoint name/route with the Collateral microservice
            .AddUrlSegmentParameter("entityType", entityType)
            .AddUrlSegmentParameter("entityId", entityId);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CollateralRequestDto>>();
    }

    // Legacy: Container.Resolve<ICollateralServiceAdapter>().ChangeTempCollateralRequest(list) — converts the request's
    // temporary collaterals into permanent ones bound to the entity.
    //
    // TODO(blocking): the legacy call passed a second argument, `true` (ExternalProxy.ChangeTempCollateralRequest(list, true);
    // other modules pass false). It is the Collateral service's `sendMessageOnChangeTempRequest`
    // (FinanceInfr\Collateral\...\CollateralRequestBL.cs:1592): it reaches the CollateralRequestCreated event as
    // AdditionalInfo = "True"/"False", and the event's handler (InternalHandleNewCollateralRequest) sends the collateral-request
    // message (COLT_NG_8211_MSG10040) only when it is true. This payload carries no such flag, so whether the message goes out
    // depends on the cloud Collateral service's default. Check, once that service exists in the cloud, whether its endpoint takes
    // the flag (and where) and pass `true` for this caller (parity finding F-08, second half).
    public async Task ChangeTempCollateralRequest(List<ChangeTempCollateralRequestDto> requests)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Collateral/ChangeTempCollateralRequest") // TODO(blocking): confirm endpoint name/route with the Collateral microservice
            .AddBody(requests);
        await ExecuteAsync(req);
    }

    // Legacy: ICollateralServiceAdapter.GetCollateralRequestIDsByRelatedEntityDTO(...) — the collateral-request ids on
    // an entity (SaveAuthenticationRequestFile).
    public async Task<List<int>?> GetCollateralRequestIdsByRelatedEntity(int entityType, int entityId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("api/Collateral/CollateralRequestIdsByEntity/{entityType}/{entityId}") // TODO(blocking): confirm endpoint name/route with the Collateral microservice
            .AddUrlSegmentParameter("entityType", entityType)
            .AddUrlSegmentParameter("entityId", entityId);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<int>>();
    }

    // Legacy: ICollateralServiceAdapter.GrantAllCollateralRequests(list) — grants all collateral requests on the entity.
    public async Task GrantAllCollateralRequests(List<GrantCollateralRequestDto> requests)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Collateral/GrantAllCollateralRequests") // TODO(blocking): confirm endpoint name/route with the Collateral microservice
            .AddBody(requests);
        await ExecuteAsync(req);
    }

    // Legacy: ICollateralServiceAdapter.DebitCreditCollateralRequest(DebitCreditFilter) — collects the guarantee on
    // a rejected authentication answer.
    public async Task DebitCreditCollateralRequest(DebitCreditCollateralRequestDto request)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Collateral/DebitCreditCollateralRequest") // TODO(blocking): confirm endpoint name/route with the Collateral microservice
            .AddBody(request);
        await ExecuteAsync(req);
    }
}
