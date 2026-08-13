using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class InternationalSiteProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), IInternationalSiteProxy
{
    // Legacy: SystemTablesUtil.GetIdByCode<InternationalSite>(PropLocode, code) → GetCodeById(id) — the port/shipment
    // fields' UN/LOCODEs are resolved to international-site ids by the SystemTables service. Batched by the message locodes.
    public async Task<List<InternationalSiteByLocodeDto>?> GetInternationalSitesByLocodes(List<string> locodes)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("InternationalSite/InternationalSitesByLocodes") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(locodes);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<InternationalSiteByLocodeDto>>();
    }
}
