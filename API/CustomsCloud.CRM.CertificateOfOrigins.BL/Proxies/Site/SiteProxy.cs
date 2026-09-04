using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class SiteProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), ISiteProxy
{
    // Legacy: SystemTablesUtil.GetIdByCode<SiteLookup>(PropExternalSiteNumberForMessages, code) →
    // GetCodeById<SiteLookup>(id).OrganizationUnitID — the CustomsHouse external site number is resolved to the site's
    // org-unit id by the SystemTables service. Batched by the external site numbers the message carries.
    public async Task<List<SiteByExternalNumberDto>?> GetSitesByExternalNumbers(List<string> externalSiteNumbers)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Site/SitesByExternalNumbers") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(externalSiteNumbers);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<SiteByExternalNumberDto>>();
    }
}
