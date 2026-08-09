using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomerProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Customers), ICustomerProxy
{
    public async Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Customer/CustomersByIds") // TODO(blocking): confirm endpoint name/route with the Customers microservice
            .AddBody(customerIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomerDto>>();
    }

    public async Task<CustomerDto?> GetCustomerInformation(int customerId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"Customer/CustomerInformation/{customerId}"); // TODO(blocking): confirm endpoint name/route with the Customers microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<CustomerDto>();
    }

    public async Task<List<CustomerDto>?> GetCustomersByCountry(int countryId)
    {
        // Foreign customs-houses in the given country. The legacy passed the fixed activity-type filter
        // ECustomerActivityType.Foreign_customs_house = 40 (בית מכס זר) alongside the country id.
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"Customer/CustomersByCountry/{countryId}/{CertificateOfOriginsConsts.ForeignCustomsHouseActivityType}"); // TODO(blocking): confirm endpoint name/route with the Customers microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomerDto>>();
    }

    public async Task<int?> GetCustomerIdByExternalId(string externalId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"Customer/IdByExternalId/{externalId}"); // TODO(blocking): confirm endpoint name/route with the Customers microservice
        var response = await ExecuteAsync(req);
        return await response.GetResult<int?>();
    }
}
