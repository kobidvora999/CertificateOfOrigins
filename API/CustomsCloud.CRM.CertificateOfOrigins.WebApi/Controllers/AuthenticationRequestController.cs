using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("AuthenticationRequest")]
public class AuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<AuthenticationRequestBl>(serviceProvider)
{
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
