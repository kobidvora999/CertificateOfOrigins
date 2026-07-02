using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class VendorMockProxy : IVendorProxy
{
    // Returns hardcoded dummy data — used while the real Vendors service endpoint is unavailable.
    public Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds)
    {
        var result = vendorIds.Select(id => new VendorDto
        {
            Id = id,                        // TODO: dummy data
            Title = "Test Vendor " + id,    // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<VendorDto>?>(result);
    }
}
