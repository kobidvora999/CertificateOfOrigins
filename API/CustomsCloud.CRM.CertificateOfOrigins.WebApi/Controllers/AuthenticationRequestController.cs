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
    [HttpPost("HandleAuthenticationRequestDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> HandleAuthenticationRequestDeliverySent([FromBody] RaiseEventArgsDto raiseEventArgs)
    {
        var result = await BusinessLayer.HandleAuthenticationRequestDeliverySent(raiseEventArgs);
        return Ok(result);
    }

    [HttpPost("HandleImportAuthenticationRequestDeliveryForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto>> HandleImportAuthenticationRequestDeliveryForImporterSent([FromBody] ImportAuthenticationRequestDto authenticationRequest)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
        return Ok(result);
    }

    [HttpPost("HandleImportAuthenticationRequestDeliveryReminderForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto>> HandleImportAuthenticationRequestDeliveryReminderForImporterSent([FromBody] ImportAuthenticationRequestDto authenticationRequest)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);
        return Ok(result);
    }

    [HttpPost("HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto>> HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent([FromBody] ImportAuthenticationFileDetailsDto authenticationFile, [FromQuery] bool isDelivery)
    {
        var result = await BusinessLayer.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile, isDelivery);
        return Ok(result);
    }

    [HttpPost("HandleSendRemindDeliverNotification")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> HandleSendRemindDeliverNotification([FromBody] ImportAuthenticationFileDetailsDto file)
    {
        var result = await BusinessLayer.HandleSendRemindDeliverNotification(file);
        return Ok(result);
    }

    [HttpPost("ChangeStatusAfterDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto>> ChangeStatusAfterDeliverySent([FromBody] ImportAuthenticationFileDetailsDto importAuthenticationRequest)
    {
        var result = await BusinessLayer.ChangeStatusAfterDeliverySent(importAuthenticationRequest);
        return Ok(result);
    }

    [HttpGet("CheckIfExistsAdditionalRequestsForImporter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForImporter([FromQuery] int importerId, [FromQuery] int? vendorId, [FromQuery] int? customerId, [FromQuery] int countryId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return Ok(result);
    }

    [HttpGet("CheckIfExistsAdditionalRequestsForVendor")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForVendor([FromQuery] int vendorId)
    {
        var result = await BusinessLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    [HttpGet("CheckImporterOfImportAuthentication")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery] int importerId)
    {
        var result = await BusinessLayer.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    [HttpGet("{documentId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto?>> AuthenticationRequest([FromRoute] int documentId)
    {
        var result = await BusinessLayer.GetAuthenticationRequestById(documentId);
        return Ok(result);
    }

    [HttpGet("File/{fileId}")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto?>> AuthenticationRequestFile([FromRoute] int fileId)
    {
        var result = await BusinessLayer.GetAuthenticationRequestFileByID(fileId);
        return Ok(result);
    }

    [HttpPost("CreateNewAuthenticationFile")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto?>> CreateNewAuthenticationFile([FromBody] List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        var result = await BusinessLayer.CreateNewAuthenticationFile(importAuthenticationRequests);
        return Ok(result);
    }

    [HttpGet("EntityDocuments")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<DocumentDto>))]
    public async Task<ActionResult<List<DocumentDto>>> EntityDocuments([FromQuery] ImportAuthenticationRequestDto importAuthenticationRequest)
    {
        var result = await BusinessLayer.GetEntityDocuments(importAuthenticationRequest);
        return Ok(result);
    }

    [HttpGet("AuthenticationRequestByFilter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<GetImportAuthenticationRequestResultDto>))]
    public async Task<ActionResult<List<GetImportAuthenticationRequestResultDto>>> AuthenticationRequestByFilter([FromQuery] ImportAuthenticationRequestFilterDto filter)
    {
        var result = await BusinessLayer.GetAuthenticationRequestByFilter(filter);
        return Ok(result);
    }
}
