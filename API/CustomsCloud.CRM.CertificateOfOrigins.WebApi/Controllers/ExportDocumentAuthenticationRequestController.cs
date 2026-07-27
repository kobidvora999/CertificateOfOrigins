using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("ExportDocumentAuthenticationRequest")]
public class ExportDocumentAuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<ExportDocumentAuthenticationRequestBl>(serviceProvider)
{
    // Internal WCF: GetExportDocumentAuthenticationRequestSearch(filter) — export-document authentication-request
    // search. Returns the matching requests (empty list when none — a search, never 404). CountryName +
    // ForeignCustomsHouseName + RequestIssuerName are enriched in the BL (Country lookup + Customers proxy);
    // DocumentType/RequestStatus/ExportDeclaration names come from local joins.
    // Internal WCF: GetCustomerInformation(customerId) — fetch a single customer's information (incl. addresses)
    // from the Customers service by id. Missing id → 404 (BL throws RestNotFoundException). The SPA picks the
    // Authentication-purpose address (else the first) from Addresses as the customs-house address.
    [HttpGet("CustomerInformation/{customerId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> CustomerInformation([FromRoute] int customerId)
    {
        var result = await BusinessLayer.GetCustomerInformation(customerId);
        return Ok(result);
    }

    // Internal WCF: GetCustomerInformationByCountry(countryId) — the foreign customs-house for a country
    // (Customers service, filtered to activity-type Foreign_customs_house). No customs-house → 404
    // (BL throws RestNotFoundException). Returns the first matching customer.
    [HttpGet("CustomerInformationByCountry/{countryId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> CustomerInformationByCountry([FromRoute] int countryId)
    {
        var result = await BusinessLayer.GetCustomerInformationByCountry(countryId);
        return Ok(result);
    }

    [HttpGet("ExportDocumentAuthenticationRequestSearch")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetExportDocumentAuthenticationRequestSearchResultDto>))]
    public async Task<ActionResult<List<GetExportDocumentAuthenticationRequestSearchResultDto>>> ExportDocumentAuthenticationRequestSearch([FromQuery] ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var result = await BusinessLayer.GetExportDocumentAuthenticationRequestSearch(filter);
        return Ok(result);
    }
}
