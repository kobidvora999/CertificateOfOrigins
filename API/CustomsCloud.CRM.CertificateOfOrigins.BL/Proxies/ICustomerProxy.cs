using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomerProxy
{
    Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds);
}
