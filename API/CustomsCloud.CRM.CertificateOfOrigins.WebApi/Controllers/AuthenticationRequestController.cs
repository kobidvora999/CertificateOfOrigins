using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.WebApi;
using CustomsCloud.InfrastructureCore.WebApi.OpenApiOperations;
using Microsoft.AspNetCore.Mvc;

namespace CustomsCloud.CRM.CertificateOfOrigins.WebApi.Controllers;

[Route("AuthenticationRequest")]
public class AuthenticationRequestController(IServiceProvider serviceProvider)
    : BaseController<AuthenticationRequestBl>(serviceProvider)
{
    // Internal WCF: GetAuthenticationRequestByFilter(filter) — import-authentication-request search. Returns the
    // matching requests (empty list when none — a search, never 404). ImporterName/VendorName are enriched via
    // proxies and IssuingCountry/OrganizationUnit names via lookups; only LeadDocument title stays null (raw id
    // returned). Supply the request-date range — the SP always applies it.
    [HttpGet("AuthenticationRequestByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>>> AuthenticationRequestByFilter([FromQuery] ImportAuthenticationRequestFilterDto filter)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestByLeadDocumentIDs(list) — fetch the import-authentication requests for
    // the given lead-document ids (bound as a Shared.IntArray TVP). The payload is a list of ids, hence POST.
    // Returns the matching requests (empty when none). ImportCountry + OrganizationUnit names are enriched in the
    // BL via lookups; LeadDocumentTitle stays null (no owning-service proxy).
    [HttpPost("AuthenticationRequestByLeadDocumentIDs")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetAuthenticationRequestByLeadDocumentResultDto>))]
    public async Task<ActionResult<List<GetAuthenticationRequestByLeadDocumentResultDto>>> AuthenticationRequestByLeadDocumentIDs([FromBody] List<int> leadDocumentIds)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByLeadDocumentIDs(leadDocumentIds);
        return Ok(result);
    }

    // Internal WCF: CheckImporterOfImportAuthentication(importerId) — returns the importer id when the importer
    // is NOT on the verification-prohibited list, or null when it is. A check (not a resource lookup) → no 404.
    [HttpGet("CheckImporterOfImportAuthentication")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery] int importerId)
    {
        var result = await BusinessLayer.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForVendor(vendorId) — true if the vendor has more than one
    // import-authentication request in the last 3 years. A check → returns bool (no 404).
    [HttpGet("CheckIfExistsAdditionalRequestsForVendor")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForVendor([FromQuery] int vendorId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    // Internal WCF: GetEntityDocuments(importAuthenticationRequest) — the WCF took the full request entity but used
    // only its LeadDocumentID, so it is flattened to a route param here. Returns the lead document's documents
    // (from the Documents service), filtered to the allowed types and to documents not already requested/claimed.
    // An empty result is legitimate (no 404).
    [HttpGet("EntityDocuments/{leadDocumentId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<DocumentDto>))]
    public async Task<ActionResult<List<DocumentDto>>> EntityDocuments([FromRoute] int leadDocumentId)
    {
        var result = await BusinessLayer.GetEntityDocuments(leadDocumentId);
        return Ok(result);
    }

    // Internal WCF: CheckIfExistsAdditionalRequestsForImporter(request) — the WCF took the full request entity
    // but used only 4 scalar fields; flattened to query params here. True if an additional import-authentication
    // request exists for the importer within the config window. A check → returns bool (no 404).
    [HttpGet("CheckIfExistsAdditionalRequestsForImporter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForImporter([FromQuery] int importerId, [FromQuery] int? vendorId, [FromQuery] int? customerId, [FromQuery] int countryId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return Ok(result);
    }

    // Internal WCF: ChangeStatusAfterDeliverySent(fileDetails) — raises CloseAllTaskForImportAuthenticationRequestFile
    // for the authentication-request file (the Events microservice closes the open tasks). A state-changing action
    // with a body → POST. The legacy full-entity input is flattened to Id + OrganizationUnitId. Returns true.
    [HttpPost("ChangeStatusAfterDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> ChangeStatusAfterDeliverySent([FromBody] ChangeStatusAfterDeliverySentRequestDto request)
    {
        var result = await BusinessLayer.ChangeStatusAfterDeliverySent(request);
        return Ok(result);
    }

    // Internal WCF: HandleSendRemindDeliverNotification(fileDetails) — the BL method is named CloseReminderTask.
    // Raises CloseTaskReminderNotice3Months for the authentication-request file (the Events microservice closes the
    // 3-month reminder task). A state-changing action with a body → POST; the full-entity input is flattened to
    // Id + OrganizationUnitId. Returns true.
    [HttpPost("CloseReminderTask")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CloseReminderTask([FromBody] CloseReminderTaskRequestDto request)
    {
        var result = await BusinessLayer.CloseReminderTask(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(file, isDelivery) — the vendor/
    // customs-house delivery flow. Advances the file's status + delivery method and stamps the delivery dates on the
    // file and its child requests. A state-changing write with a body → POST. Returns the file's new status + method.
    [HttpPost("HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(HandleDeliveryAndReminderForVendorSentResultDto))]
    public async Task<ActionResult<HandleDeliveryAndReminderForVendorSentResultDto>> HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent([FromBody] HandleDeliveryAndReminderForVendorSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryForImporterSent(request) — the importer delivery flow.
    // Stamps the request's decision + date, advances the parent file's status machine (touching the file + its child
    // requests), and raises the NewDeliveryForImporterSent event. A state-changing write with a body → POST.
    [HttpPost("HandleImportAuthenticationRequestDeliveryForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(HandleDeliveryOrReminderForImporterSentResultDto))]
    public async Task<ActionResult<HandleDeliveryOrReminderForImporterSentResultDto>> HandleImportAuthenticationRequestDeliveryForImporterSent([FromBody] HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryForImporterSent(request);
        return Ok(result);
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request) — the importer reminder
    // flow. Same behaviour as the importer delivery endpoint, with the reminder event + decision. A write → POST.
    [HttpPost("HandleImportAuthenticationRequestDeliveryReminderForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(HandleDeliveryOrReminderForImporterSentResultDto))]
    public async Task<ActionResult<HandleDeliveryOrReminderForImporterSentResultDto>> HandleImportAuthenticationRequestDeliveryReminderForImporterSent([FromBody] HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request);
        return Ok(result);
    }

    // Internal WCF: CreateNewAuthenticationFile(requests) — creates a new import authentication-request file from a
    // set of requests and links them to it. Fails with 400 (RestValidationException) if any request already belongs
    // to a file. A state-changing write with a body → POST. Returns the created file.
    [HttpPost("CreateNewAuthenticationFile")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CreateNewAuthenticationFileResultDto))]
    public async Task<ActionResult<CreateNewAuthenticationFileResultDto>> CreateNewAuthenticationFile([FromBody] List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        var result = await BusinessLayer.CreateNewAuthenticationFile(importAuthenticationRequests);
        return Ok(result);
    }

    // Internal WCF: GetAuthenticationRequestByID(documentId) — a single import authentication request with its item
    // lines, decision lookup, collaterals (Collateral service), and current-user task flags (Tasks service). Missing
    // id → 404 (BL throws RestNotFoundException). The lead Document + LeadDocumentSubmissionDate are cross-service and
    // deferred (null).
    [HttpGet("AuthenticationRequestByID/{documentId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(GetAuthenticationRequestByIdResultDto))]
    public async Task<ActionResult<GetAuthenticationRequestByIdResultDto>> AuthenticationRequestByID([FromRoute] int documentId)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByID(documentId);
        return Ok(result);
    }
}
