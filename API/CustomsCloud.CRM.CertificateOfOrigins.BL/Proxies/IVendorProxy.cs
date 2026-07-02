using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IVendorProxy
{
    Task<List<VendorDto>?> GetVendorsByIds(List<int> vendorIds);
}
