using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("CertificateOfOrigin")]
public class CertificateOfOriginController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginBl>(serviceProvider)
{
    [HttpPost("Convert")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(VirtualEntityDto))]
    public async Task<ActionResult<VirtualEntityDto>> Convert([FromBody] ConnectedEntityDto connectedEntity)
    {
        var result = await BusinessLayer.Convert(connectedEntity);
        return Ok(result);
    }

    // route-style (alternate key) WITHOUT the 404 contract — callers rely on null when the number is unknown (C1, developer decision)
    [HttpGet("CertificateOfOriginID/{certificateNumber}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CertificateOfOriginID([FromRoute] string certificateNumber)
    {
        var result = await BusinessLayer.GetCertificateOfOriginID(certificateNumber);
        return Ok(result);
    }

    // the payload is a list of DTOs — bound from the body, hence POST
    [HttpPost("GoodsItemCerificateDTO")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GoodsItemCerificateDto>))]
    public async Task<ActionResult<List<GoodsItemCerificateDto>>> GoodsItemCerificateDTO([FromBody] List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        var result = await BusinessLayer.GetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
        return Ok(result);
    }

    [HttpPost("CertificateOfOriginAttachments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CertificateOfOriginAttachments([FromBody] SaveCertificateAttachmentsArgsDto saveCertificateAttachmentsArgsDto)
    {
        var result = await BusinessLayer.SaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDto);
        return Ok(result);
    }

    [HttpGet("AuthenticationRequestByLeadDocumentIDs")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<ImportAuthenticationRequestByLeadDocumentDto>))]
    public async Task<ActionResult<List<ImportAuthenticationRequestByLeadDocumentDto>>> AuthenticationRequestByLeadDocumentIDs([FromQuery] List<int> leadDocumentIDs)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
        return Ok(result);
    }

    [HttpGet("{certificateOfOriginId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto?>> CertificateOfOrigin([FromRoute] int certificateOfOriginId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginById(certificateOfOriginId);
        return Ok(result);
    }

    [HttpGet("CertificateOfOriginByExternalIdExist")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginResultDto))]
    public async Task<ActionResult<CertificateOfOriginResultDto?>> CertificateOfOriginByExternalIdExist([FromQuery] string certificateOfOriginExternalId)
    {
        var result = await BusinessLayer.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
        return Ok(result);
    }

    [HttpGet("CertificateOfOriginsByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginResultDto>))]
    public async Task<ActionResult<List<CertificateOfOriginResultDto>>> CertificateOfOriginsByFilter([FromQuery] CertificateOfOriginFilterDto filter)
    {
        var result = await BusinessLayer.GetCertificateOfOriginsByFilter(filter);
        return Ok(result);
    }

    // Legacy WCF: External UpdateCetrificateOfOrigins (IsOneWay) — event-driven certificate updates from
    // the export-declaration flow. POST: the event dto travels in the body; returns 200 on completion.
    [HttpPost("UpdateCetrificateOfOrigins")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> UpdateCetrificateOfOrigins([FromBody] UpdateCetificateOfOriginsDto updateCetificateOfOrigins)
    {
        await BusinessLayer.UpdateCetrificateOfOrigins(updateCetificateOfOrigins);
        return Ok(true);
    }

    // POST (not GET despite the "Load" prefix): the full certificate travels in the body and is returned enriched.
    [HttpPost("LoadDataFromExportDeclaration")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto>> LoadDataFromExportDeclaration([FromBody] CertificateOfOriginDto certificateOfOrigin)
    {
        var result = await BusinessLayer.LoadDataFromExportDeclaration(certificateOfOrigin);
        return Ok(result);
    }

    // generic template contract (C1) — consumed by the Templates flow and internal-workload-test
    [HttpGet("TemplateData/{templateId}/{entityId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(PrintTemplateDto))]
    public async Task<ActionResult<PrintTemplateDto>> TemplateData([FromRoute] int templateId, [FromRoute] int entityId)
    {
        var result = await BusinessLayer.GetTemplateData(templateId, entityId);
        return Ok(result);
    }

    [HttpGet("GenerateTemplate/{templateId}/{entityId}")]
    [BadRequestResponse][NotFoundResponse]
    public async Task<IActionResult> GenerateTemplate([FromRoute] int templateId, [FromRoute] int entityId)
    {
        var stream = await BusinessLayer.GenerateTemplate(templateId, entityId);
        return File(stream, "application/pdf");
    }
}
