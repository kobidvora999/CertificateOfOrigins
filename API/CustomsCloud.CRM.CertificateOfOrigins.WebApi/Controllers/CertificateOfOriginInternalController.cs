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
    [HttpPost("HandleImportAuthenticationRequestDeliveryForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto>> HandleImportAuthenticationRequestDeliveryForImporterSent([FromBody] ImportAuthenticationRequestDto authenticationRequest)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.HandleImportAuthenticationRequestDeliveryForImporterSent(authenticationRequest);
        return Ok(result);
    }

    [HttpPost("HandleImportAuthenticationRequestDeliveryReminderForImporterSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationRequestDto))]
    public async Task<ActionResult<ImportAuthenticationRequestDto>> HandleImportAuthenticationRequestDeliveryReminderForImporterSent([FromBody] ImportAuthenticationRequestDto authenticationRequest)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.HandleImportAuthenticationRequestDeliveryReminderForImporterSent(authenticationRequest);
        return Ok(result);
    }

    [HttpPost("HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto>> HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent([FromBody] ImportAuthenticationFileDetailsDto authenticationFile, [FromQuery] bool isDelivery)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(authenticationFile, isDelivery);
        return Ok(result);
    }

    [HttpPost("HandleSendRemindDeliverNotification")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> HandleSendRemindDeliverNotification([FromBody] ImportAuthenticationFileDetailsDto file)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.HandleSendRemindDeliverNotification(file);
        return Ok(result);
    }

    [HttpPost("ChangeStatusAfterDeliverySent")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ImportAuthenticationFileDetailsDto))]
    public async Task<ActionResult<ImportAuthenticationFileDetailsDto>> ChangeStatusAfterDeliverySent([FromBody] ImportAuthenticationFileDetailsDto importAuthenticationRequest)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.ChangeStatusAfterDeliverySent(importAuthenticationRequest);
        return Ok(result);
    }

    [HttpGet("CheckIfExistsAdditionalRequestsForImporter")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForImporter([FromQuery] int importerId, [FromQuery] int? vendorId, [FromQuery] int? customerId, [FromQuery] int countryId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return Ok(result);
    }

    [HttpGet("CheckIfExistsAdditionalRequestsForVendor")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(bool))]
    public async Task<ActionResult<bool>> CheckIfExistsAdditionalRequestsForVendor([FromQuery] int vendorId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return Ok(result);
    }

    [HttpGet("CheckImporterOfImportAuthentication")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(int?))]
    public async Task<ActionResult<int?>> CheckImporterOfImportAuthentication([FromQuery] int importerId)
    {
        var bl = serviceProvider.GetRequiredService<AuthenticationRequestBl>();
        var result = await bl.CheckImporterOfImportAuthentication(importerId);
        return Ok(result);
    }

    [HttpGet("GetCustomerInformation")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> GetCustomerInformation([FromQuery] int customerId)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetCustomerInformation(customerId);
        return Ok(result);
    }

    [HttpGet("GetCustomerInformationByCountry")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(CustomerDto))]
    public async Task<ActionResult<CustomerDto>> GetCustomerInformationByCountry([FromQuery] int countryId)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetCustomerInformationByCountry(countryId);
        return Ok(result);
    }

    [HttpGet("GetExportDocumentAuthenticationRequestByID")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(ExportDocumentAuthenticationRequestDto))]
    public async Task<ActionResult<ExportDocumentAuthenticationRequestDto?>> GetExportDocumentAuthenticationRequestByID([FromQuery] int id)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetExportDocumentAuthenticationRequestByID(id);
        return Ok(result);
    }

    [HttpGet("GetExportDocumentAuthenticationRequestSearch")]
    [BadRequestResponse][NotFoundResponse][OkJsonResponse(typeof(List<ExportDocumentAuthenticationRequestSearchResultDto>))]
    public async Task<ActionResult<List<ExportDocumentAuthenticationRequestSearchResultDto>>> GetExportDocumentAuthenticationRequestSearch([FromQuery] ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var bl = serviceProvider.GetRequiredService<ExportDocumentAuthenticationRequestBl>();
        var result = await bl.GetExportDocumentAuthenticationRequestSearch(filter);
        return Ok(result);
    }

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
