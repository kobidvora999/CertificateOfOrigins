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
}
