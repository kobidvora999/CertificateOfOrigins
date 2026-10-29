using Api.Models.CertificateOfOriginsCommunity;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.WebApi.Filters;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Community;

// C14 Schema First, pure REST. GetPC_MSG2280_2281_CertificateOfOriginRequest is an incoming message from a customs agent;
// the domain `community` (the customs-agent community) was a developer decision, 2026-09-05. Authoritative contract:
// .spec/OpenApi/certificateOfOriginsCommunity.json (repo root); the request/response types are generated from it.
// Alternative 2 (customs-approved 28.10.2026): the one legacy message, which chose its operation by RequestReasonCode, is
// split into an endpoint and HTTP method per operation, with the certificate number in the URL.
[ApiController]
[ProxyFailureExceptionFilter]
[Route("community/certificate-of-origins")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // Legacy request reasons 1, 2, 10, 12 without a certificate number.
    [HttpPost]
    [BadRequestResponse]
    [CreatedResponse(typeof(CertificateOfOriginResult))]
    public async Task<ActionResult<CertificateOfOriginResult>> Create([FromBody] CertificateOfOriginRequest request)
    {
        var result = await BusinessLayer.CreateCertificateOfOriginFromMessage(request);
        return Created(string.Empty, result);
    }

    // Legacy request reason 13.
    [HttpGet("{certificateId}")]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CertificateOfOriginResult))]
    public async Task<ActionResult<CertificateOfOriginResult>> Status([FromRoute] string certificateId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginStatusForMessage(certificateId);
        return Ok(result);
    }

    // Legacy request reasons 1, 2, 12 with a certificate number, and 3.
    [HttpPut("{certificateId}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [ConflictResponse]
    [OkJsonResponse(typeof(CertificateOfOriginResult))]
    public async Task<ActionResult<CertificateOfOriginResult>> Update([FromRoute] string certificateId, [FromBody] CertificateOfOriginRequest request)
    {
        var result = await BusinessLayer.UpdateCertificateOfOriginFromMessage(certificateId, request);
        return Ok(result);
    }

    // Legacy request reason 14.
    [HttpDelete("{certificateId}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [ConflictResponse]
    [OkJsonResponse(typeof(CertificateOfOriginResult))]
    public async Task<ActionResult<CertificateOfOriginResult>> Cancel([FromRoute] string certificateId)
    {
        var result = await BusinessLayer.CancelCertificateOfOriginByMessage(certificateId);
        return Ok(result);
    }

    // Legacy request reasons 4 and 5.
    [HttpPost("{certificateIdToCancel}/replacement")]
    [BadRequestResponse]
    [NotFoundResponse]
    [ConflictResponse]
    [CreatedResponse(typeof(CertificateOfOriginResult))]
    public async Task<ActionResult<CertificateOfOriginResult>> Replacement([FromRoute] string certificateIdToCancel, [FromBody] CertificateOfOriginRequest request)
    {
        var result = await BusinessLayer.ReplaceCertificateOfOriginFromMessage(certificateIdToCancel, request);
        return Created(string.Empty, result);
    }
}
