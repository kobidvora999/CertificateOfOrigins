using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomerProxy
{
    Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds);

    Task<CustomerDto?> GetCustomerInformation(int customerId);

    Task<List<CustomerDto>?> GetCustomersByCountry(int countryId);
}
