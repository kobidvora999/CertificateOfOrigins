using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomerMockProxy : ICustomerProxy
{
    // Returns hardcoded dummy data — used while the real Customers service endpoint is unavailable.
    public Task<CustomerDto?> GetCustomerByIdentification(int customerId)
    {
        return Task.FromResult<CustomerDto?>(new CustomerDto
        {
            Id = customerId,          // TODO: dummy data
            Name = "Test Customer",   // TODO: dummy data
            ExternalIdNum = "000000000",
            IsActive = true,
        });
    }

    public Task<List<CustomerDto>?> GetCustomersByCountryId(int countryId, int customerActivityTypeId)
    {
        return Task.FromResult<List<CustomerDto>?>(new List<CustomerDto>
        {
            new()
            {
                Id = 1,                          // TODO: dummy data
                Name = "Test Customs House",     // TODO: dummy data
                CountryId = countryId,
                IsActive = true,
            }
        });
    }
}
