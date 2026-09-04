using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomsBookProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), ICustomsBookProxy
{
    public async Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isCountryGroup)
    {
        // TODO(blocking): the CustomsBook / trade-agreement service is not yet stood up — confirm the owning
        // microservice + endpoint route (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/CustomsBook/IsTradeAgreementForCountry/{certificateTypeId}/{countryId}/{isCountryGroup}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }

    public async Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters)
    {
        // TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice +
        // endpoint route (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CustomsBook/CustomsItemsByIds")
            .AddBody(filters);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomsItemDto>>();
    }

    public async Task<int?> GetCustomsItemIdByFullClassification(string fullClassification)
    {
        // TODO(blocking): the CustomsBook customs-item service is not yet stood up — confirm the owning microservice +
        // endpoint route (until then the mock is enabled via x-mock-mode). "Export" is the fixed book type the legacy
        // create-branch passes (ECustomsBookType.Export).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"api/CustomsBook/CustomsItemIdByFullClassification/Export/{fullClassification}");
        var response = await ExecuteAsync(req);
        return await response.GetResult<int?>();
    }
}
