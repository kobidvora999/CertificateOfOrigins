using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomsBookProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.CustomsBook), ICustomsBookProxy // TODO(blocking): confirm CustomsMicroServices.CustomsBook exists in the enum
{
    public async Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isImport)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("CustomsBook/IsTradeAgreementForCountry") // TODO(blocking): confirm endpoint name against the CustomsBook service
            .AddQueryStringParameter("certificateTypeId", certificateTypeId)
            .AddQueryStringParameter("countryId", countryId)
            .AddQueryStringParameter("isImport", isImport);
        return (await ExecuteAsync<bool>(req)).Data;
    }

    public async Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("CustomsBook/CustomsItemsByIds") // TODO(blocking): confirm endpoint name against the CustomsBook service
            .AddBody(filters);
        return (await ExecuteAsync<List<CustomsItemDto>>(req)).Data;
    }
}
