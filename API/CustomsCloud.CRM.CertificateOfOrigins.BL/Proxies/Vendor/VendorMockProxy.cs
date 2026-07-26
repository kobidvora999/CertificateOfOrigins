using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class VendorMockProxy(IProxyMockUtil mockUtil) : IVendorProxy, IMockProxy
{
    // Default = realistic dummy vendors; feature "Vendors.NotFound" flips to the not-found branch.
    public Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds)
    {
        if (mockUtil.HasMockFeature("Vendors.NotFound"))
        {
            return Task.FromResult<List<VendorDto>?>(null);
        }

        var result = vendorIds.Select(id => new VendorDto
        {
            Id = id,                       // TODO: dummy data
            Name = "Test Vendor " + id,    // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<VendorDto>?>(result);
    }
}
