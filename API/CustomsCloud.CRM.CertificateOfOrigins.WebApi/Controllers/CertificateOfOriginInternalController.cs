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
    [HttpGet("GetAuthenticationRequestByLeadDocumentIDs")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<ImportAuthenticationRequestByLeadDocumentDto>))]
    public async Task<ActionResult<List<ImportAuthenticationRequestByLeadDocumentDto>>> GetAuthenticationRequestByLeadDocumentIDs([FromQuery] List<int> leadDocumentIDs)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIDs);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestByID")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto?>> GetAuthenticationRequestByID([FromQuery] int documentId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestById(documentId);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestFileByID")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto?>> GetAuthenticationRequestFileByID([FromQuery] int fileId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestFileByID(fileId);
        return Ok(result);
    }

    [HttpPost("CreateNewAuthenticationFile")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto?>> CreateNewAuthenticationFile([FromBody] List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CreateNewAuthenticationFile(importAuthenticationRequests);
        return Ok(result);
    }

    [HttpGet("GetEntityDocuments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<DocumentDto>))]
    public async Task<ActionResult<List<DocumentDto>>> GetEntityDocuments([FromQuery] ImportAuthenticationRequestDto importAuthenticationRequest)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetEntityDocuments(importAuthenticationRequest);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>>> GetAuthenticationRequestByFilter([FromQuery] ImportAuthenticationRequestFilterDto filter)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }

    [HttpGet("GetCertificateOfOriginById")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto?>> GetCertificateOfOriginById([FromQuery] int certificateOfOriginId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginById(certificateOfOriginId);
        return Ok(result);
    }

    [HttpGet("IsCertificateOfOriginByExternalIdExist")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginResultDto))]
    public async Task<ActionResult<CertificateOfOriginResultDto?>> IsCertificateOfOriginByExternalIdExist([FromQuery] string certificateOfOriginExternalId)
    {
        var result = await BusinessLayer.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
        return Ok(result);
    }

    [HttpGet("GetCertificateOfOriginsByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginResultDto>))]
    public async Task<ActionResult<List<CertificateOfOriginResultDto>>> GetCertificateOfOriginsByFilter([FromQuery] CertificateOfOriginFilterDto filter)
    {
        var result = await BusinessLayer.GetCertificateOfOriginsByFilter(filter);
        return Ok(result);
    }
}
