using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("External")]
public class CertificateOfOriginExternalController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginBl>(serviceProvider)
{
    [HttpPost("Convert")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(VirtualEntityDto))]
    public async Task<ActionResult<VirtualEntityDto>> Convert([FromBody] ConnectedEntityDto connectedEntity)
    {
        var result = await BusinessLayer.Convert(connectedEntity);
        return Ok(result);
    }

    [HttpGet("GetCertificateOfOriginID")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> GetCertificateOfOriginID([FromQuery] string certificateNumber)
    {
        var result = await BusinessLayer.GetCertificateOfOriginID(certificateNumber);
        return Ok(result);
    }

    // legacy contract prefix is "Get" but the payload is a list of DTOs — bound from the body
    [HttpPost("GetGoodsItemCerificateDTO")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GoodsItemCerificateDto>))]
    public async Task<ActionResult<List<GoodsItemCerificateDto>>> GetGoodsItemCerificateDTO([FromBody] List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        var result = await BusinessLayer.GetGoodsItemCerificateDTO(goodsItemCerificateDTOs);
        return Ok(result);
    }

    [HttpPost("SaveCertificateOfOriginAttachments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> SaveCertificateOfOriginAttachments([FromBody] SaveCertificateAttachmentsArgsDto saveCertificateAttachmentsArgsDto)
    {
        var result = await BusinessLayer.SaveCertificateOfOriginAttachments(saveCertificateAttachmentsArgsDto);
        return Ok(result);
    }

    [HttpPost("HandleAuthenticationRequestDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> HandleAuthenticationRequestDeliverySent([FromBody] RaiseEventArgsDto raiseEventArgs)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.HandleAuthenticationRequestDeliverySent(raiseEventArgs);
        return Ok(result);
    }
}
