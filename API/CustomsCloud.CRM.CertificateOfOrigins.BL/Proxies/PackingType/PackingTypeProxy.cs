using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class PackingTypeProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), IPackingTypeProxy
{
    // Legacy: SystemTablesUtil.GetIdByCode<PackingType>(PropCommonCode, code) — the invoice item PackageType codes are
    // resolved to packing-type ids by the SystemTables service. Batched by the message codes.
    public async Task<List<PackingTypeByCodeDto>?> GetPackingTypesByCodes(List<string> commonCodes)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/PackingType/PackingTypesByCodes") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(commonCodes);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<PackingTypeByCodeDto>>();
    }
}
