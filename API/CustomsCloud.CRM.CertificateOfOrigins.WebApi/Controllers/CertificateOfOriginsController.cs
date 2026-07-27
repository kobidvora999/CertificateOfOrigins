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
}
