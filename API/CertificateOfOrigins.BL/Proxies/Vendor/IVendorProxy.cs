using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IVendorProxy
{
    Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds);
}
