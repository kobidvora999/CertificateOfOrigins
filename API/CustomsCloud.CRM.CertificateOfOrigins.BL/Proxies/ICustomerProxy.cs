using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomerProxy
{
    Task<CustomerDto?> GetCustomerByIdentification(int customerId);

    Task<List<CustomerDto>?> GetCustomersByCountryId(int countryId, int customerActivityTypeId);
}
