using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomerMockProxy(IProxyMockUtil mockUtil) : ICustomerProxy, IMockProxy
{
    // Default = realistic dummy customers; feature "Customers.NotFound" flips to the not-found branch.
    public Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds)
    {
        if (mockUtil.HasMockFeature("Customers.NotFound"))
        {
            return Task.FromResult<List<CustomerDto>?>(null);
        }

        var result = customerIds.Select(id => new CustomerDto
        {
            Id = id,                          // TODO: dummy data
            Name = "Test Customer " + id,     // TODO: dummy data
            ExternalIdNum = "000000000",
            IsActive = true,
        }).ToList();
        return Task.FromResult<List<CustomerDto>?>(result);
    }

    public Task<CustomerDto?> GetCustomerInformation(int customerId)
    {
        if (mockUtil.HasMockFeature("Customers.NotFound"))
        {
            return Task.FromResult<CustomerDto?>(null);
        }

        var result = new CustomerDto
        {
            Id = customerId,                  // TODO: dummy data
            Name = "Test Customer " + customerId,
            ExternalIdNum = "000000000",
            IsActive = true,
            Addresses =
            [
                new CustomerAddressDto { AddressPurpose = 1, AddressSingleLine = "רחוב הבדיקה 1, חיפה" }, // TODO: dummy authentication address
            ],
        };
        return Task.FromResult<CustomerDto?>(result);
    }
}
