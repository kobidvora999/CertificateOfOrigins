using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class VendorProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Vendors), IVendorProxy
{
    public async Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Vendor/VendorsByIds") // TODO(blocking): confirm endpoint name/route with the Vendors microservice
            .AddBody(vendorIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<VendorDto>>();
    }
}
