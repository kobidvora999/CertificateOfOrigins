using CertificateOfOrigins.BL;
using CertificateOfOrigins.WebApi.Filters;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CertificateOfOrigins.WebApi.Controllers.Ui;

[ApiController]
[ProxyFailureExceptionFilter]
[Route("ui/authentication-request")]
public class AuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<AuthenticationRequestBl>(serviceProvider)
{
    // Internal WCF: GetAuthenticationRequestByFilter(filter) — import-authentication-request search. Returns the
    // matching requests (empty list when none — a search, never 404). ImporterName/VendorName are enriched via
    // proxies and IssuingCountry/OrganizationUnit names via lookups; only LeadDocument title stays null (raw id
    // returned). Supply the request-date range — the SP always applies it.
    [HttpQuery]
    [BadRequestResponse]
    [OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>>> Search([FromBody] ImportAuthenticationRequestFilterDto filter)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestByLeadDocumentIDs(list) — fetch the import-authentication requests for
    // the given lead-document ids (bound as a Shared.IntArray TVP). A read whose input is a collection → QUERY.
    // Returns the matching requests (empty when none). ImportCountry + OrganizationUnit names are enriched in the
    // BL via lookups; LeadDocumentTitle stays null (no owning-service proxy).
    [HttpQuery("by-lead-document-ids")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<GetAuthenticationRequestByLeadDocumentResultDto>))]
    public async Task<ActionResult<List<GetAuthenticationRequestByLeadDocumentResultDto>>> ByLeadDocumentIds([FromBody] List<int> leadDocumentIds)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIds);
        return Ok(result);
    }

    // Internal WCF: CheckImporterOfImportAuthentication(importerId) — returns the importer id when the importer
    // is NOT on the verification-prohibited list, or null when it is. A check (not a resource lookup) → no 404.
    [HttpGet("check-importer-of-import-authentication")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery][BindRequired] int importerId)
    {
        var result = await BusinessLayer.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForVendor(vendorId) — true if the vendor has more than one
    // import-authentication request in the last 3 years. A check → returns bool (no 404). The route drops "requests":
    // the controller is the request.
    [HttpGet("additional-exist-for-vendor")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> AdditionalExistForVendor([FromQuery][BindRequired] int vendorId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    // Internal WCF: GetEntityDocuments(importAuthenticationRequest) — the WCF took the full request entity but used
    // only its LeadDocumentID, so it is flattened to a route param here. Returns the lead document's documents
    // (from the Documents service), filtered to the allowed types and to documents not already requested/claimed.
    // An empty result is legitimate (no 404).
    [HttpGet("entity-documents/{leadDocumentId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(List<DocumentDto>))]
    public async Task<ActionResult<List<DocumentDto>>> EntityDocuments([FromRoute] int leadDocumentId)
    {
        var result = await BusinessLayer.GetEntityDocuments(leadDocumentId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForImporter(request) — the WCF took the full request entity
    // but used only 4 scalar fields; flattened to query params here. True if an additional import-authentication
    // request exists for the importer within the config window. A check → returns bool (no 404).
    [HttpGet("additional-exist-for-importer")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> AdditionalExistForImporter([FromQuery][BindRequired] int importerId, [FromQuery] int? vendorId, [FromQuery] int? customerId, [FromQuery][BindRequired] int countryId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return Ok(result);
    }

    // Internal WCF: ChangeStatusAfterDeliverySent(fileDetails) — raises CloseAllTaskForImportAuthenticationRequestFile
    // for the authentication-request file (the Events microservice closes the open tasks). A state-changing action
    // with a body → POST. The legacy full-entity input is flattened to Id + OrganizationUnitId. Returns true.
    [HttpPost("change-status-after-delivery-sent")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> ChangeStatusAfterDeliverySent([FromBody] ChangeStatusAfterDeliverySentRequestDto request)
    {
        var result = await BusinessLayer.ChangeStatusAfterDeliverySent(request);
        return Ok(result);
    }

    // Internal WCF: HandleSendRemindDeliverNotification(fileDetails) — the BL method is named CloseReminderTask.
    // Raises CloseTaskReminderNotice3Months for the authentication-request file (the Events microservice closes the
    // 3-month reminder task). A state-changing action with a body → POST; the full-entity input is flattened to
    // Id + OrganizationUnitId. Returns true.
    [HttpPost("close-reminder-task")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CloseReminderTask([FromBody] CloseReminderTaskRequestDto request)
    {
        var result = await BusinessLayer.CloseReminderTask(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(file, isDelivery) — the vendor/
    // customs-house delivery flow. Advances the file's status + delivery method and stamps the delivery dates on the
    // file and its child requests. A state-changing write with a body → POST. Returns the file's new status + method.
    [HttpPost("handle-import-delivery-and-reminder-for-vendor-sent")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(HandleDeliveryAndReminderForVendorSentResultDto))]
    public async Task<ActionResult<HandleDeliveryAndReminderForVendorSentResultDto>> HandleImportDeliveryAndReminderForVendorSent([FromBody] HandleDeliveryAndReminderForVendorSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryForImporterSent(request) — the importer delivery flow.
    // Stamps the request's decision + date, advances the parent file's status machine (touching the file + its child
    // requests), and raises the NewDeliveryForImporterSent event. A state-changing write with a body → POST.
    [HttpPost("handle-import-delivery-for-importer-sent")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(HandleDeliveryOrReminderForImporterSentResultDto))]
    public async Task<ActionResult<HandleDeliveryOrReminderForImporterSentResultDto>> HandleImportDeliveryForImporterSent([FromBody] HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryForImporterSent(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request) — the importer reminder
    // flow. Same behaviour as the importer delivery endpoint, with the reminder event + decision. A write → POST.
    [HttpPost("handle-import-delivery-reminder-for-importer-sent")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(HandleDeliveryOrReminderForImporterSentResultDto))]
    public async Task<ActionResult<HandleDeliveryOrReminderForImporterSentResultDto>> HandleImportDeliveryReminderForImporterSent([FromBody] HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request);
        return Ok(result);
    }

    // Internal WCF: CreateNewAuthenticationFile(requests) — creates a new import authentication-request file from a
    // set of requests and links them to it. Fails with 400 (RestValidationException) if any request already belongs
    // to a file. Creates a file → POST on file. Returns the created file. The C# names keep "AuthenticationFile":
    // a method named File would hide ControllerBase.File (CS0108).
    [HttpPost("file")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(CreateNewAuthenticationFileResultDto))]
    public async Task<ActionResult<CreateNewAuthenticationFileResultDto>> CreateNewAuthenticationFile([FromBody] List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        var result = await BusinessLayer.CreateNewAuthenticationFile(importAuthenticationRequests);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestByID(documentId) — a single import authentication request with its item
    // lines, decision lookup, collaterals (Collateral service), and current-user task flags (Tasks service). Missing
    // id → 404 (BL throws RestNotFoundException). The lead Document + LeadDocumentSubmissionDate are cross-service and
    // deferred (null).
    [HttpGet("{documentId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(GetAuthenticationRequestByIdResultDto))]
    public async Task<ActionResult<GetAuthenticationRequestByIdResultDto>> AuthenticationRequestByID([FromRoute] int documentId)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByID(documentId);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestFileByID(fileId) — a single authentication file with its child requests
    // (each enriched), the file-status lookup, and the current-user handling flag. Missing id → 404 (BL throws
    // RestNotFoundException).
    [HttpGet("file/{fileId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(GetAuthenticationRequestFileByIdResultDto))]
    public async Task<ActionResult<GetAuthenticationRequestFileByIdResultDto>> AuthenticationRequestFileByID([FromRoute] int fileId)
    {
        var result = await BusinessLayer.GetAuthenticationRequestFileByID(fileId);
        return Ok(result);
    }

    // Internal WCF: SaveImportAuthenticationRequest(request) — saves an import authentication request's central-decision
    // edits: pushes its collaterals to permanent (Collateral service), raises the decision-driven events + message, and
    // inserts the request when the caller sends IsNewInstance, otherwise updates it (missing row → 404). A
    // write with a body → POST on the controller root (one route for insert and update, as legacy; no "save" in routes).
    // Returns the fully re-read request graph (same shape as GetAuthenticationRequestByID).
    [HttpPost]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(GetAuthenticationRequestByIdResultDto))]
    public async Task<ActionResult<GetAuthenticationRequestByIdResultDto>> SaveImportAuthenticationRequest([FromBody] SaveImportAuthenticationRequestRequestDto request)
    {
        var result = await BusinessLayer.SaveImportAuthenticationRequest(request);
        return Ok(result);
    }

    // Internal WCF: SaveAuthenticationRequestFile(file) — saves an authentication file's central-decision review:
    // persists each child request's decision, raises the per-request + per-file status events, sends the decision /
    // status messages, and grants collaterals on approval. Updates the file → PUT file/{fileId}; the body's Id must be
    // the same file (else 400). Missing file row → 404. Returns the fully re-read file (same shape as
    // GetAuthenticationRequestFileByID).
    [HttpPut("file/{fileId:int}")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(GetAuthenticationRequestFileByIdResultDto))]
    public async Task<ActionResult<GetAuthenticationRequestFileByIdResultDto>> SaveAuthenticationRequestFile([FromRoute] int fileId, [FromBody] SaveAuthenticationRequestFileRequestDto request)
    {
        if (request.Id != fileId)
        {
            throw new RestValidationException(nameof(request.Id), ValidationMessages.RouteIdDoesNotMatchBody, [fileId, request.Id]);
        }

        var result = await BusinessLayer.SaveAuthenticationRequestFile(request);
        return Ok(result);
    }

    // Internal WCF: GetPathsForNavigationToVendor() — the navigation-path tree for the fixed "navigate to vendor"
    // PathID. A read with no input → GET. Data source: the NavigationPath reference table — pending a platform Lookup
    // type; the BL returns an empty ViewPaths list until it is wired (see the BL TODO + INTERNAL_INTEGRATION.md).
    [HttpGet("paths-for-navigation-to-vendor")]
    [BadRequestResponse]
    [NotFoundResponse]
    [OkJsonResponse(typeof(NavigationToVendorViewDto))]
    public async Task<ActionResult<NavigationToVendorViewDto>> PathsForNavigationToVendor()
    {
        var result = await BusinessLayer.GetPathsForNavigationToVendor();
        return Ok(result);
    }
}
