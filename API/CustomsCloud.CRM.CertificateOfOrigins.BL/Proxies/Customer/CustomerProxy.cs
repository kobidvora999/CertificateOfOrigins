using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomerProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Customers), ICustomerProxy
{
    public async Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Customer/CustomersByIds") // TODO(blocking): confirm endpoint name/route with the Customers microservice
            .AddBody(customerIds);
        var result = await ExecuteAsync<List<CustomerDto>>(req);
        return result.Data;
    }
}
