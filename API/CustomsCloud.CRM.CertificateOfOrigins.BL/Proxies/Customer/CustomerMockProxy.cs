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
}
