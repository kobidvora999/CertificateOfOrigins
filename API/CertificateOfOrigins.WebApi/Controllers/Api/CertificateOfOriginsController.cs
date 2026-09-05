using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Api;

// C14: the `api` (external / service-to-service) surface of CertificateOfOriginsBl — everything the legacy exposed
// as an External WCF operation. Callers reach these under the api/ prefix, which is also what C15 makes every
// outbound proxy route carry.
[ApiController]
[Route("api/[controller]")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // External WCF: GetCertificateOfOriginID(certificateNumber) — route-style alternate key; returns the latest
    // certificate id for the given number. Missing number → 404 (BL throws RestNotFoundException).
    [HttpGet("ID/{certificateNumber}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int))]
    public async Task<ActionResult<int>> CertificateOfOriginID([FromRoute] string certificateNumber)
    {
        var result = await BusinessLayer.GetCertificateOfOriginID(certificateNumber);
        return Ok(result);
    }

    // External WCF: GetGoodsItemCerificateDTO(list) — enriches each item with its certificate id (latest by
    // number); the payload is a list of DTOs bound from the body, hence POST. Per-item miss → null (no 404).
    [HttpPost("GoodsItemCerificateDTO")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GoodsItemCerificateDto>))]
    public async Task<ActionResult<List<GoodsItemCerificateDto>>> GoodsItemCerificateDTO([FromBody] List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        var result = await BusinessLayer.GetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
        return Ok(result);
    }

    // External WCF: Convert(connectedEntity) — the ESB/EAI entity-resolution operation. Resolves the connected
    // entity's key (EntityIdKey1 = certificate number) to a generic VirtualEntity link. Missing certificate → 404
    // (BL throws RestNotFoundException). POST because it takes the ConnectedEntity payload in the body.
    [HttpPost("Convert")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(VirtualEntityDto))]
    public async Task<ActionResult<VirtualEntityDto>> Convert([FromBody] ConnectedEntityDto connectedEntity)
    {
        var result = await BusinessLayer.Convert(connectedEntity);
        return Ok(result);
    }

    // External WCF: SaveCertificateOfOriginAttachments(args) — saves the generated certificate template(s) as
    // attachments on the certificate, replacing whatever documents are currently attached. A state-changing write
    // with a body → POST. Returns true.
    [HttpPost("SaveAttachments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> SaveCertificateOfOriginAttachments([FromBody] SaveCertificateAttachmentsArgsDto request)
    {
        var result = await BusinessLayer.SaveCertificateOfOriginAttachments(request);
        return Ok(result);
    }

    // External WCF: UpdateCetrificateOfOrigins(dto) — the export-declaration → certificate reconciliation (a one-way
    // DealFile event). Reconciles each certificate against the declaration: sets DeclarationMatch / Rejected, raises
    // the matching event, and re-prints the draft. A state-changing write with a body → POST. The legacy contract is
    // one-way/void; here the reconciliation errors are surfaced (developer decision) — empty list when all matched.
    [HttpPost("Reconcile")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginExceptionDto>))]
    public async Task<ActionResult<List<CertificateOfOriginExceptionDto>>> UpdateCertificateOfOrigins([FromBody] UpdateCertificateOfOriginsRequestDto request)
    {
        var result = await BusinessLayer.UpdateCertificateOfOrigins(request);
        return Ok(result);
    }
}
