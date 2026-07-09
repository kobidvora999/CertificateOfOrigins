using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("ExportDocumentAuthenticationRequest")]
public class ExportDocumentAuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<ExportDocumentAuthenticationRequestBl>(serviceProvider)
{
    [HttpGet("CustomerInformation/{customerId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> CustomerInformation([FromRoute] int customerId)
    {
        var result = await BusinessLayer.GetCustomerInformation(customerId);
        return Ok(result);
    }

    [HttpGet("CustomerInformationByCountry/{countryId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> CustomerInformationByCountry([FromRoute] int countryId)
    {
        var result = await BusinessLayer.GetCustomerInformationByCountry(countryId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ExportDocumentAuthenticationRequestDto))]
    public async Task<ActionResult<ExportDocumentAuthenticationRequestDto?>> ExportDocumentAuthenticationRequest([FromRoute] int id)
    {
        var result = await BusinessLayer.GetExportDocumentAuthenticationRequestByID(id);
        return Ok(result);
    }

    [HttpGet("ExportDocumentAuthenticationRequestSearch")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<ExportDocumentAuthenticationRequestSearchResultDto>))]
    public async Task<ActionResult<List<ExportDocumentAuthenticationRequestSearchResultDto>>> ExportDocumentAuthenticationRequestSearch([FromQuery] ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var result = await BusinessLayer.GetExportDocumentAuthenticationRequestSearch(filter);
        return Ok(result);
    }
}
