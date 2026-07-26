using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("AuthenticationRequest")]
public class AuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<AuthenticationRequestBl>(serviceProvider)
{
    // Internal WCF: GetAuthenticationRequestByFilter(filter) — import-authentication-request search. Returns the
    // matching requests (empty list when none — a search, never 404). ImporterName/VendorName are enriched via
    // proxies and IssuingCountry/OrganizationUnit names via lookups; only LeadDocument title stays null (raw id
    // returned). Supply the request-date range — the SP always applies it.
    [HttpGet("AuthenticationRequestByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>>> AuthenticationRequestByFilter([FromQuery] ImportAuthenticationRequestFilterDto filter)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestByLeadDocumentIDs(list) — fetch the import-authentication requests for
    // the given lead-document ids (bound as a Shared.IntArray TVP). The payload is a list of ids, hence POST.
    // Returns the matching requests (empty when none). ImportCountry + OrganizationUnit names are enriched in the
    // BL via lookups; LeadDocumentTitle stays null (no owning-service proxy).
    [HttpPost("AuthenticationRequestByLeadDocumentIDs")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetAuthenticationRequestByLeadDocumentResultDto>))]
    public async Task<ActionResult<List<GetAuthenticationRequestByLeadDocumentResultDto>>> AuthenticationRequestByLeadDocumentIDs([FromBody] List<int> leadDocumentIds)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIds);
        return Ok(result);
    }

    // Internal WCF: CheckImporterOfImportAuthentication(importerId) — returns the importer id when the importer
    // is NOT on the verification-prohibited list, or null when it is. A check (not a resource lookup) → no 404.
    [HttpGet("CheckImporterOfImportAuthentication")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery] int importerId)
    {
        var result = await BusinessLayer.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForVendor(vendorId) — true if the vendor has more than one
    // import-authentication request in the last 3 years. A check → returns bool (no 404).
    [HttpGet("CheckIfExistsAdditionalRequestsForVendor")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForVendor([FromQuery] int vendorId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForImporter(request) — the WCF took the full request entity
    // but used only 4 scalar fields; flattened to query params here. True if an additional import-authentication
    // request exists for the importer within the config window. A check → returns bool (no 404).
    [HttpGet("CheckIfExistsAdditionalRequestsForImporter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForImporter([FromQuery] int importerId, [FromQuery] int? vendorId, [FromQuery] int? customerId, [FromQuery] int countryId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return Ok(result);
    }
}
