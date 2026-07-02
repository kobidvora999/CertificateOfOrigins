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
            .WithResource("Internal/GetCustomersByIds") // TODO: confirm endpoint name with the Customers microservice
            .AddBody(customerIds);
        return (await ExecuteAsync<List<CustomerDto>>(req)).Data;
    }
}
