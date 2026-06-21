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

    [HttpGet("GetCustomerInformation")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto?>> GetCustomerInformation([FromQuery] int customerId)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetCustomerInformation(customerId);
        return Ok(result);
    }

    [HttpGet("GetCustomerInformationByCountry")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto?>> GetCustomerInformationByCountry([FromQuery] int countryId)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetCustomerInformationByCountry(countryId);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestByID")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto?>> GetAuthenticationRequestByID([FromQuery] int documentId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestById(documentId);
        return Ok(result);
    }

    [HttpGet("CheckIfExistsAdditionalRequestsForVendor")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForVendor([FromQuery] int vendorId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    [HttpGet("CheckImporterOfImportAuthentication")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery] int importerId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    [HttpGet("GetAuthenticationRequestFileByID")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto?>> GetAuthenticationRequestFileByID([FromQuery] int fileId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.GetAuthenticationRequestFileById(fileId);
        return Ok(result);
    }
}
