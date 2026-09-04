using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CurrencyTypeProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), ICurrencyTypeProxy
{
    // Legacy: SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode — currency codes live in the SystemTables
    // service. Batched by ids (the invoice CurrencyTypeIDs used by the web query).
    public async Task<List<CurrencyTypeDto>?> GetCurrencyTypesByIds(List<int> currencyTypeIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CurrencyType/CurrencyTypesByIds") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(currencyTypeIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CurrencyTypeDto>>();
    }

    // Legacy: SystemTablesUtil.GetIdByCode<CurrencyType>(PropCurrencyCode, code) — the invoice CurrencyType codes are
    // resolved to currency-type ids by the SystemTables service (the create-branch invoice conversion). Batched by codes.
    public async Task<List<CurrencyTypeDto>?> GetCurrencyTypesByCodes(List<string> currencyCodes)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CurrencyType/CurrencyTypesByCodes") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(currencyCodes);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CurrencyTypeDto>>();
    }
}
