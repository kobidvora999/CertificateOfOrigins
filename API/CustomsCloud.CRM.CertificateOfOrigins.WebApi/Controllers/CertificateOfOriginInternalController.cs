using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("Internal")]
public class CertificateOfOriginInternalController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginBl>(serviceProvider)
{
    [HttpGet("GetCertificateOfOriginsByFilter")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<CertificateOfOriginResultDto>))]
    public async Task<ActionResult<List<CertificateOfOriginResultDto>?>> GetCertificateOfOriginsByFilter([FromQuery] CertificateOfOriginFilterDto filter)
    {
        var result = await BusinessLayer.GetCertificateOfOriginsByFilter(filter);
        return Ok(result);
    }

    [HttpGet("IsCertificateOfOriginByExternalIdExist")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CertificateOfOriginResultDto))]
    public async Task<ActionResult<CertificateOfOriginResultDto?>> IsCertificateOfOriginByExternalIdExist([FromQuery] string certificateOfOriginExternalId)
    {
        var result = await BusinessLayer.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
        return Ok(result);
    }

    [HttpGet("GetCertificateOfOriginById")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto?>> GetCertificateOfOriginById([FromQuery] int certificateOfOriginId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginById(certificateOfOriginId);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestByFilter")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>?>> GetAuthenticationRequestByFilter([FromQuery] ImportAuthenticationRequestFilterDto filter)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }
}
