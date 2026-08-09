using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("CertificateOfOrigins")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // Internal WCF: IsCertificateOfOriginByExternalIdExist(externalId) — existence query by certificate number
    // (LIKE substring, newest match). Returns the matching result, or null when none — existence check, no 404.
    [HttpGet("CertificateOfOriginByExternalIdExist")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginResultDto))]
    public async Task<ActionResult<CertificateOfOriginResultDto?>> CertificateOfOriginByExternalIdExist([FromQuery] string certificateOfOriginExternalId)
    {
        var result = await BusinessLayer.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
        return Ok(result);
    }

    // External WCF: GetCertificateOfOriginID(certificateNumber) — route-style alternate key; returns the latest
    // certificate id for the given number. Missing number → 404 (BL throws RestNotFoundException).
    [HttpGet("CertificateOfOriginID/{certificateNumber}")]
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

    // Internal WCF: GetCertificateOfOriginsByFilter(filter) — the main certificate search. Returns the matching
    // certificates (empty list when none — a search, never 404). The BL/DAL/SP/enrichment were built for #2.
    [HttpGet("CertificateOfOriginsByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginResultDto>))]
    public async Task<ActionResult<List<CertificateOfOriginResultDto>>> CertificateOfOriginsByFilter([FromQuery] CertificateOfOriginFilterDto filter)
    {
        var result = await BusinessLayer.GetCertificateOfOriginsByFilter(filter);
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

    // Internal WCF: GetCertificateOfOriginById(id) — a single certificate with its full graph (header + declaration
    // errors + details + invoices + item lines + milestones), from the 7-result-set dbo.GetCertificateOfOriginByID.
    // Missing id → 404 (BL throws RestNotFoundException). Milestone user names are enriched in the BL via IUserProxy.
    [HttpGet("CertificateOfOriginById/{certificateOfOriginId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto>> CertificateOfOriginById([FromRoute] int certificateOfOriginId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginById(certificateOfOriginId);
        return Ok(result);
    }

    // Incoming/portal WCF: GetCertificateRequestByGuid (GetPC_Web_9096_CertificateRequest) — certificate
    // verification for the public portal, located by guid or by CertificateOfOriginNumber + IssuingDate. Returns
    // the web-query response. The legacy in-band error contract is preserved: an invalid guid or no matching
    // certificate returns an HTTP 200 with ExceptionDescription set (not a 404), so the external portal is unaffected.
    [HttpGet("CertificateRequestByGuid")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginsResponseDto))]
    public async Task<ActionResult<CertificateOfOriginsResponseDto>> CertificateRequestByGuid([FromQuery] CertificateOfOriginsRequestDto request)
    {
        var result = await BusinessLayer.GetCertificateRequestByGuid(request);
        return Ok(result);
    }

    // Internal WCF: LoadDataFromExportDeclaration(certificateOfOrigin) — looks up the certificate's export
    // declaration in the ExportDealFile service and returns whether it may proceed (cargo exited customs
    // regulation AND the request is not a retrospective certificate). The legacy also mutated the entity
    // by-reference (IsDeclarationReleased/IsCargoExitedOfCustomsRegulation); over REST only the flag is returned.
    [HttpGet("LoadDataFromExportDeclaration")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> LoadDataFromExportDeclaration([FromQuery] LoadDataFromExportDeclarationRequestDto request)
    {
        var result = await BusinessLayer.LoadDataFromExportDeclaration(request);
        return Ok(result);
    }

    // External WCF: SaveCertificateOfOriginAttachments(args) — saves the generated certificate template(s) as
    // attachments on the certificate, replacing whatever documents are currently attached. A state-changing write
    // with a body → POST. Returns true.
    [HttpPost("SaveCertificateOfOriginAttachments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> SaveCertificateOfOriginAttachments([FromBody] SaveCertificateAttachmentsArgsDto request)
    {
        var result = await BusinessLayer.SaveCertificateOfOriginAttachments(request);
        return Ok(result);
    }

    // Internal WCF: SaveCertificateOfOrigin(certificate) — inserts (Id == 0) or updates a certificate of origin + its
    // detail rows: supersedes the previous version, validates/enriches the details, generates the QR + template
    // attachments on publish, links the DealFile lead document, and raises the status-change events + feedback message.
    // A state-changing write with a body → POST. Returns the fully re-read certificate graph (GetCertificateOfOriginById).
    [HttpPost("SaveCertificateOfOrigin")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto>> SaveCertificateOfOrigin([FromBody] SaveCertificateOfOriginRequestDto request)
    {
        var result = await BusinessLayer.SaveCertificateOfOrigin(request);
        return Ok(result);
    }

    // External WCF: UpdateCetrificateOfOrigins(dto) — the export-declaration → certificate reconciliation (a one-way
    // DealFile event). Reconciles each certificate against the declaration: sets DeclarationMatch / Rejected, raises
    // the matching event, and re-prints the draft. A state-changing write with a body → POST. The legacy contract is
    // one-way/void; here the reconciliation errors are surfaced (developer decision) — empty list when all matched.
    [HttpPost("UpdateCertificateOfOrigins")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginExceptionDto>))]
    public async Task<ActionResult<List<CertificateOfOriginExceptionDto>>> UpdateCertificateOfOrigins([FromBody] UpdateCertificateOfOriginsRequestDto request)
    {
        var result = await BusinessLayer.UpdateCertificateOfOrigins(request);
        return Ok(result);
    }

    // Generic template-data endpoint (one per microservice): the merged PrintTemplateDto (structure + values) for a
    // template id + certificate id. Route-style single resource → missing entity is a 404 (BL throws RestNotFoundException).
    [HttpGet("TemplateData/{templateId}/{entityId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(PrintTemplateDto))]
    public async Task<ActionResult<PrintTemplateDto>> TemplateData([FromRoute] int templateId, [FromRoute] int entityId)
    {
        var result = await BusinessLayer.GetTemplateData(templateId, entityId);
        return Ok(result);
    }

    // Generic render endpoint (one per microservice): the rendered PDF for a template id + certificate id.
    [HttpGet("GenerateTemplate/{templateId}/{entityId}")]
    [BadRequestResponse][NotFoundResponse]
    public async Task<IActionResult> GenerateTemplate([FromRoute] int templateId, [FromRoute] int entityId)
    {
        var stream = await BusinessLayer.GenerateTemplate(templateId, entityId);
        return File(stream, "application/pdf", $"{templateId}_{entityId}.pdf");
    }
}
