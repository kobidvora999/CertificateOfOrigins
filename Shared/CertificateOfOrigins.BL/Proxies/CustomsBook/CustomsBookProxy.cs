using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomsBookProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.CustomsBook), ICustomsBookProxy
{
    // Legacy CustomsBookServicesAdapter.IsTradeAgreementForCountry → KnowledgeStore.CustomsBook, one trade agreement at a
    // time, as of now.
    public async Task<bool> IsTradeAgreementForCountry(int countryOrGroupId, int tradeAgreementId, bool isGroup)
    {
        // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CustomsBook/IsTradeAgreementForCountry")
            .AddBody(new IfExistTradeAgreementForCountryFilterDto
            {
                TradeAgreementId = tradeAgreementId,
                CountryOrGroupId = countryOrGroupId,
                IsGroup = isGroup,
                ReferenceDate = DateTime.Now,
            });
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }

    public async Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters)
    {
        // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode).
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CustomsBook/CustomsItemsByIds")
            .AddBody(filters);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomsItemDto>>();
    }

    public async Task<int?> GetCustomsItemIdByFullClassification(string fullClassification)
    {
        // TODO(blocking): confirm the CustomsBook endpoint route (until then the mock is enabled via x-mock-mode). "Export"
        // is the fixed book type the legacy create-branch passes (ECustomsBookType.Export); legacy also sent DateTime.Today
        // as the reference date — TODO(confirm): how the CustomsBook route takes it.
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("api/CustomsBook/CustomsItemIdByFullClassification/Export/{fullClassification}")
            .AddUrlSegmentParameter("fullClassification", fullClassification);
        var response = await ExecuteWithoutValidationAsync(req);
        response.Validate(HttpStatusCode.NotFound); // not found → null, as the legacy adapter returned
        return await response.GetResult<int?>();
    }
}
