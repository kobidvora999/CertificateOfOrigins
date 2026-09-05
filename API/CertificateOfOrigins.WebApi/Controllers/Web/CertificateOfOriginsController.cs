using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Web;

// C14 Schema First. The domain comes from the legacy service name: GetPC_Web_9096_CertificateRequest contains
// "Web", so this incoming message belongs to the `web` domain. Its contract is committed by hand at
// .spec/OpenApi/web-certificaterequest.openapi.json (derived from the legacy message schema) and guarded by
// Test/SchemaContract/WebCertificateRequestContractTests.cs — the controller is written to match the contract,
// not the other way round.
[ApiController]
[Route("web/[controller]")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // Incoming/portal WCF: GetCertificateRequestByGuid (GetPC_Web_9096_CertificateRequest) — certificate
    // verification for the public portal, located by guid or by CertificateOfOriginNumber + IssuingDate. Returns
    // the web-query response. The legacy in-band error contract is preserved: an invalid guid or no matching
    // certificate returns an HTTP 200 with ExceptionDescription set (not a 404), so the external portal is unaffected.
    [HttpGet("RequestByGuid")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginsResponseDto))]
    public async Task<ActionResult<CertificateOfOriginsResponseDto>> CertificateRequestByGuid([FromQuery] CertificateOfOriginsRequestDto request)
    {
        var result = await BusinessLayer.GetCertificateRequestByGuid(request);
        return Ok(result);
    }
}
