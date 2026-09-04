using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CountryProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), ICountryProxy
{
    // Legacy: SystemTablesUtil.GetIdByCode<Country>(Country.PropCountryAlphaCode_2, code) — country alpha-2 codes are
    // resolved to ids by the SystemTables service. Batched by the codes the incoming certificate message carries.
    public async Task<List<CountryByCodeDto>?> GetCountriesByAlphaCodes(List<string> alphaCodes)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Country/CountriesByAlphaCodes") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(alphaCodes);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CountryByCodeDto>>();
    }
}
