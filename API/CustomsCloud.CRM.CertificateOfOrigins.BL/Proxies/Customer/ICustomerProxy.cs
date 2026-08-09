using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomerProxy
{
    Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds);

    Task<CustomerDto?> GetCustomerInformation(int customerId);

    Task<List<CustomerDto>?> GetCustomersByCountry(int countryId);

    // Legacy: servicesAdapter.GetCustomerIDByExternalID(externalId) (SaveCertificateOfOrigin, ExporterId validation) —
    // the internal customer id for an external customer id, or null when the customer is unknown.
    Task<int?> GetCustomerIdByExternalId(string externalId);
}
