using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class VendorProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Vendors), IVendorProxy
{
    public async Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Vendor/VendorsByIds") // TODO: confirm endpoint name (route = target BL name) with the Vendors microservice
            .AddBody(vendorIds);
        return (await ExecuteAsync<List<VendorDto>>(req)).Data;
    }
}
