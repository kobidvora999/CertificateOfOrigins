using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CurrencyTypeProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.SystemTables), ICurrencyTypeProxy
{
    // Legacy: SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode — currency codes live in the SystemTables
    // service. Batched by ids (the invoice CurrencyTypeIDs used by the web query).
    public async Task<List<CurrencyTypeDto>?> GetCurrencyTypesByIds(List<int> currencyTypeIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("CurrencyType/CurrencyTypesByIds") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(currencyTypeIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CurrencyTypeDto>>();
    }
}
