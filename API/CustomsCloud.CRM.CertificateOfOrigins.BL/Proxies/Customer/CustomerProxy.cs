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
            .WithResource("Customer/CustomersByIds") // TODO: confirm endpoint name (route = target BL name) with the Customers microservice
            .AddBody(customerIds);
        return (await ExecuteAsync<List<CustomerDto>>(req)).Data;
    }

    public async Task<CustomerDto?> GetCustomerByIdentification(int customerId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/CustomerByIdentification") // TODO: confirm endpoint name (route = target BL name) with the Customers microservice
            .AddQueryStringParameter("customerId", customerId);
        return (await ExecuteAsync<CustomerDto>(req)).Data;
    }

    public async Task<List<CustomerDto>?> GetCustomersByCountryId(int countryId, int customerActivityTypeId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/CustomeDTOByCountryID") // TODO: confirm endpoint name (route = target BL name) with the Customers microservice
            .AddQueryStringParameter("countryId", countryId)
            .AddQueryStringParameter("customerActivityTypeId", customerActivityTypeId);
        return (await ExecuteAsync<List<CustomerDto>>(req)).Data;
    }
}
