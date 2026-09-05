using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Community;

// C14 Schema First. GetPC_MSG2280_2281_CertificateOfOriginRequest is an incoming message from a customs agent,
// and its legacy name carries neither WEB nor FRM, so the C14 classification rule does not settle it — the
// domain `community` (the customs-agent community) was a developer decision, 2026-09-05. Its contract is
// committed by hand at .spec/OpenApi/community-certificateoforiginrequest.openapi.json and guarded by
// Test/SchemaContract/CommunityCertificateOfOriginRequestContractTests.cs.
[ApiController]
[Route("community/[controller]")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // Incoming/EAI WCF: GetPC_MSG2280_2281_CertificateOfOriginRequest — an agent's certificate-of-origin request message.
    // Exposed as a synchronous POST that returns the feedback directly (the legacy one-way callback/MSMQ response is
    // replaced by a direct return — mirrors the legacy *Sync contract). A message body with side effects → POST + body.
    [HttpPost("Request")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginRequestFeedbackResponseDto))]
    public async Task<ActionResult<CertificateOfOriginRequestFeedbackResponseDto>> CertificateOfOriginRequest([FromBody] CertificateOfOriginRequestMessageDto request)
    {
        var result = await BusinessLayer.GetPC22802281CertificateOfOriginRequest(request);
        return Ok(result);
    }
}
