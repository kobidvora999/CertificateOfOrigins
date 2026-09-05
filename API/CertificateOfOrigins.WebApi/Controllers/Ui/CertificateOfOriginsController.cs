using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CertificateOfOrigins.WebApi.Controllers.Ui;

// C14: the `ui` (internal) surface of CertificateOfOriginsBl. The class is named after the BL, not after a
// resource — the same name recurs in Api/, Web/ and Community/, kept apart by folder + namespace.
[ApiController]
[Route("ui/[controller]")]
public class CertificateOfOriginsController(IServiceProvider serviceProvider)
    : BaseController<CertificateOfOriginsBl>(serviceProvider)
{
    // Internal WCF: IsCertificateOfOriginByExternalIdExist(externalId) — existence query by certificate number
    // (LIKE substring, newest match). Returns the matching result, or null when none — existence check, no 404.
    [HttpGet("ByExternalIdExist")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginResultDto))]
    public async Task<ActionResult<CertificateOfOriginResultDto?>> CertificateOfOriginByExternalIdExist([FromQuery][BindRequired] string certificateOfOriginExternalId)
    {
        var result = await BusinessLayer.IsCertificateOfOriginByExternalIdExist(certificateOfOriginExternalId);
        return Ok(result);
    }

    // Internal WCF: GetCertificateOfOriginsByFilter(filter) — the main certificate search. Returns the matching
    // certificates (empty list when none — a search, never 404). The BL/DAL/SP/enrichment were built for #2.
    [HttpQuery("ByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<CertificateOfOriginResultDto>))]
    public async Task<ActionResult<List<CertificateOfOriginResultDto>>> CertificateOfOriginsByFilter([FromBody] CertificateOfOriginFilterDto filter)
    {
        var result = await BusinessLayer.GetCertificateOfOriginsByFilter(filter);
        return Ok(result);
    }

    // Internal WCF: GetCertificateOfOriginById(id) — a single certificate with its full graph (header + declaration
    // errors + details + invoices + item lines + milestones), from the 7-result-set dbo.GetCertificateOfOriginByID.
    // Missing id → 404 (BL throws RestNotFoundException). Milestone user names are enriched in the BL via IUserProxy.
    [HttpGet("{certificateOfOriginId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto>> CertificateOfOriginById([FromRoute] int certificateOfOriginId)
    {
        var result = await BusinessLayer.GetCertificateOfOriginById(certificateOfOriginId);
        return Ok(result);
    }

    // Internal WCF: LoadDataFromExportDeclaration(certificateOfOrigin) — looks up the certificate's export
    // declaration in the ExportDealFile service and returns whether it may proceed (cargo exited customs
    // regulation AND the request is not a retrospective certificate). The legacy also mutated the entity
    // by-reference (IsDeclarationReleased/IsCargoExitedOfCustomsRegulation); over REST only the flag is returned.
    [HttpQuery("LoadDataFromExportDeclaration")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> LoadDataFromExportDeclaration([FromBody] LoadDataFromExportDeclarationRequestDto request)
    {
        var result = await BusinessLayer.LoadDataFromExportDeclaration(request);
        return Ok(result);
    }

    // Internal WCF: SaveCertificateOfOrigin(certificate) — inserts (Id == 0) or updates a certificate of origin + its
    // detail rows: supersedes the previous version, validates/enriches the details, generates the QR + template
    // attachments on publish, links the DealFile lead document, and raises the status-change events + feedback message.
    // A state-changing write with a body → POST. Returns the fully re-read certificate graph (GetCertificateOfOriginById).
    [HttpPost]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CertificateOfOriginDto))]
    public async Task<ActionResult<CertificateOfOriginDto>> SaveCertificateOfOrigin([FromBody] SaveCertificateOfOriginRequestDto request)
    {
        var result = await BusinessLayer.SaveCertificateOfOrigin(request);
        return Ok(result);
    }
}
