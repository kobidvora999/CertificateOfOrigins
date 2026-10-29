using CertificateOfOrigins.BL;
using CertificateOfOrigins.WebApi.Filters;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CertificateOfOrigins.WebApi.Controllers.Api;

// C14: the `api` (external / service-to-service) surface of CertificateOfOriginsBl — everything the legacy exposed
// as an External WCF operation. Callers reach these under the api/ prefix, which is also what C15 makes every
// outbound proxy route carry.
[ApiController]
[ProxyFailureExceptionFilter]
[Route("api/certificate-of-origins")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // External WCF: GetCertificateOfOriginID(certificateNumber) — route-style alternate key; returns the latest
    // certificate id for the given number. Missing number → 404 (BL throws RestNotFoundException).
    [HttpGet("id/{certificateNumber}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(int))]
    public async Task<ActionResult<int>> Id([FromRoute] string certificateNumber)
    {
        var result = await BusinessLayer.GetCertificateOfOriginID(certificateNumber);
        return Ok(result);
    }

    // External WCF: GetGoodsItemCerificateDTO(list) — enriches each item with its certificate id (latest by
    // number); the payload is a list of DTOs bound from the body, hence POST. Per-item miss → null (no 404).
    [HttpPost("goods-item-certificate")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<GoodsItemCerificateDto>))]
    public async Task<ActionResult<List<GoodsItemCerificateDto>>> GoodsItemCertificate([FromBody] List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        var result = await BusinessLayer.GetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
        return Ok(result);
    }

    // External WCF: Convert(connectedEntity) — the ESB/EAI entity-resolution operation. Resolves the connected
    // entity's key (EntityIdKey1 = certificate number) to a generic VirtualEntity link. Missing certificate → 404
    // (BL throws RestNotFoundException). POST because it takes the ConnectedEntity payload in the body.
    [HttpPost("convert")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(VirtualEntityDto))]
    public async Task<ActionResult<VirtualEntityDto>> Convert([FromBody] ConnectedEntityDto connectedEntity)
    {
        var result = await BusinessLayer.Convert(connectedEntity);
        return Ok(result);
    }

    // External WCF: SaveCertificateOfOriginAttachments(args) — saves the generated certificate template(s) as
    // attachments on the certificate, replacing whatever documents are currently attached. A state-changing write
    // with a body → POST on attachments (verbless: the HTTP verb is the save). Returns true.
    [HttpPost("attachments")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> Attachments([FromBody] SaveCertificateAttachmentsArgsDto request)
    {
        var result = await BusinessLayer.SaveCertificateOfOriginAttachments(request);
        return Ok(result);
    }

    // External WCF: UpdateCetrificateOfOrigins(dto) — the export-declaration → certificate reconciliation (a one-way
    // DealFile event). Reconciles each certificate against the declaration: sets DeclarationMatch / Rejected, raises
    // the matching event, and re-prints the draft. A state-changing write with a body → POST. The legacy contract is
    // one-way/void; here the reconciliation errors are surfaced (developer decision) — empty list when all matched.
    [HttpPost("reconcile")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<CertificateOfOriginExceptionDto>))]
    public async Task<ActionResult<List<CertificateOfOriginExceptionDto>>> Reconcile([FromBody] UpdateCertificateOfOriginsRequestDto request)
    {
        var result = await BusinessLayer.UpdateCertificateOfOrigins(request);
        return Ok(result);
    }

    // CR 194221: render a template through the Templates microservice and return the document. This is the surface the
    // distribution ("דיוור") flow calls — it names the template and the entity, this service fetches the data and
    // hands back the rendered file. Route-style read of a derived resource → GET. Unregistered template id → 400
    // (RestValidationException); an entity with no data → 404 (RestNotFoundException). additionalInfo is the legacy
    // Object Maker @AdditionalInfo, passed through to the template-data procedure; the user is taken from the request.
    // freeText is the user's free text for a template that allows it (legacy TemplateArgument.FreeText).
    [HttpGet("template/{templateId:int}/{entityId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    public async Task<IActionResult> Template([FromRoute] int templateId, [FromRoute] int entityId, [FromQuery] string? additionalInfo, [FromQuery] string? freeText)
    {
        var result = await BusinessLayer.GenerateTemplate(templateId, entityId, additionalInfo, null, freeText);
        return File(result, "application/pdf");
    }

    // The generic template contract (every service): the Templates payload for a template + entity — the registered
    // template name, its data and the output format — without rendering it. Same 400/404 as Template.
    [HttpGet("template-data/{templateId:int}/{entityId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(PrintTemplateDto))]
    public async Task<ActionResult<PrintTemplateDto>> TemplateData([FromRoute] int templateId, [FromRoute] int entityId, [FromQuery] string? additionalInfo, [FromQuery] string? freeText)
    {
        var result = await BusinessLayer.GetTemplateData(templateId, entityId, additionalInfo, null, freeText);
        return Ok(result);
    }
}
