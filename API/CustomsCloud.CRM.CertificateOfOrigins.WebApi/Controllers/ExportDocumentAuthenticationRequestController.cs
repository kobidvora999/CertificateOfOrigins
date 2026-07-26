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
    // Internal WCF: GetExportDocumentAuthenticationRequestSearch(filter) — export-document authentication-request
    // search. Returns the matching requests (empty list when none — a search, never 404). CountryName +
    // ForeignCustomsHouseName + RequestIssuerName are enriched in the BL (Country lookup + Customers proxy);
    // DocumentType/RequestStatus/ExportDeclaration names come from local joins.
    [HttpGet("ExportDocumentAuthenticationRequestSearch")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetExportDocumentAuthenticationRequestSearchResultDto>))]
    public async Task<ActionResult<List<GetExportDocumentAuthenticationRequestSearchResultDto>>> ExportDocumentAuthenticationRequestSearch([FromQuery] ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var result = await BusinessLayer.GetExportDocumentAuthenticationRequestSearch(filter);
        return Ok(result);
    }
}
