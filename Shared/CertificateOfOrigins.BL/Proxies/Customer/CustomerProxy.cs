using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomerProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Customers), ICustomerProxy
{
    public async Task<List<CustomerDto>?> GetCustomersByIds(List<int> customerIds)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/customers");
      
        foreach (var id in customerIds)
        {
            req.AddQueryStringParameter("customerIds", id);
        }

        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomerDto>?>();
    }

    public async Task<CustomerDto?> GetCustomerInformation(int customerId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/BuildCustomerDTO")
            .AddQueryStringParameter("CustomerId", customerId);
        var response = await ExecuteWithoutValidationAsync(req);
        response.Validate(HttpStatusCode.NotFound); // not found → null, as the legacy adapter returned
        return await response.GetResult<CustomerDto>();
    }

    public async Task<List<CustomerDto>?> GetCustomersByCountry(int countryId)
    {
        // Foreign customs-houses in the given country. The legacy passed the fixed activity-type filter
        // ECustomerActivityType.Foreign_customs_house = 40 (בית מכס זר) alongside the country id.
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/customers")
            .AddUrlSegmentParameter("countryId", countryId)
            .AddUrlSegmentParameter("activityType", CertificateOfOriginsConsts.ForeignCustomsHouseActivityType);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<CustomerDto>>();
    }

    public async Task<int?> GetCustomerIdByExternalId(string externalId)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Customer/GetCustomerIdByExternalId/{customerExternalId}")
            .AddUrlSegmentParameter("customerExternalId", externalId);
        var response = await ExecuteWithoutValidationAsync(req);
        response.Validate(HttpStatusCode.NotFound); // not found → null, as the legacy adapter returned
        return await response.GetResult<int?>();
    }
}
