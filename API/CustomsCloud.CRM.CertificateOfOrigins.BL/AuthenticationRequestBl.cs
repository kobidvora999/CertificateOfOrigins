using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Users;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICollateralProxy collateralProxy,
    ITasksProxy tasksProxy,
    ILookupUtil lookupUtil,
    IParametersUtil parametersUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    private static DynamicParameters BuildParameterForProcedure(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrefernceDocumentType", filter.PrefernceDocumentType, DbType.Int32);
        parameters.Add("@GoodsOrigionCountry", filter.GoodsOrigionCountry, DbType.Int32);
        parameters.Add("@IssuingCountry", filter.IssuingCountry, DbType.Int32);
        parameters.Add("@ImportCountry", filter.ImportCountry, DbType.Int32);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTimeOffset);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTimeOffset);
        parameters.Add("@CustomsHouseId", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@RequestReason", filter.RequestReason, DbType.Int32);
        parameters.Add("@LeadDocumentId", filter.LeadDocumentId, DbType.Int32);
        parameters.Add("@ImporterId", filter.ImporterId, DbType.Int32);
        parameters.Add("@VendorId", filter.VendorId, DbType.Int32);
        parameters.Add("@DecisionId", filter.DecisionId, DbType.Int32);
        parameters.Add("@CustomerId", filter.CustomerId, DbType.Int32);
        parameters.Add("@DocumentId", filter.DocumentId, DbType.Int32);
        parameters.Add("@InvoiceNumber", filter.InvoiceNumber, DbType.String);
        parameters.Add("@DocumentNumber", filter.DocumentNumber, DbType.String);
        parameters.Add("@AuthenticationFileId", filter.AuthenticationFileId, DbType.Int32);
        parameters.Add("@CreateUserId", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>?> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetAuthenticationRequestByFilter(parameters);
        return result;
    }

    public async Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(int documentId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@DocumentID", documentId, DbType.Int32);

        var importAuthenticationRequest = await DataLayer.GetAuthenticationRequestById(parameters);
        if (importAuthenticationRequest == null)
        {
            return null;
        }

        await FillDocumentFileUrl(importAuthenticationRequest);
        await GetAdditionalInfoForRequest(importAuthenticationRequest);

        importAuthenticationRequest.AdditionalRequestsForSearchInDays =
            await parametersUtil.Get<int>("AdditionalRequestsForSearchInDays");
        importAuthenticationRequest.IsVendorByIssuingCountryId =
            await DataLayer.IsVendorCountry(importAuthenticationRequest.IssuingCountryId);

        return importAuthenticationRequest;
    }

    private async Task FillDocumentFileUrl(ImportAuthenticationRequestDto request)
    {
        if (request.Document != null)
        {
            var documentType = await lookupUtil.Get<DocumentType>(request.Document.TypeId);
            request.Document.FileUrl = documentType?.Name;
        }
    }

    private async Task GetAdditionalInfoForRequest(ImportAuthenticationRequestDto request)
    {
        request.Decisions = await DataLayer.GetCertificateOfOriginsDecisions() ?? new List<CertificateOfOriginsDecisionDto>();

        var collaterals = await collateralProxy.GetCollateralRequest(
            12384 /* EEntityType.ImportAuthenticationRequest */, request.DocumentId, null);
        if (collaterals != null && collaterals.Count > 0)
        {
            request.Collaterals = collaterals;
        }

        var tasks = await GetTaskDetailsForRequestByTaskType(request, new List<int>
        {
            406, // ETaskType.SetDecisionBeforeAssociation
            404, // ETaskType.SendReminderForImporter
            407  // ETaskType.HandleRejectedAuthenticationRequest
        });

        var userUtil = Resolve<IUserUtil>();
        var currentUserId = (await userUtil.GetUserId(RequestMetadata)).GetValueOrDefault();
        request.IsCurrentUserHandleRequest = tasks.Any(t => t.UserId == currentUserId);
        request.IsCurrentUserHasOpenTask = tasks.Any(t => t.UserId == currentUserId && t.IsTaskInProgress);

        request.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            { 1055 /* EEntityType.ImportDeclaration */, new List<int> { request.LeadDocumentId } }
        };
    }

    private async Task<List<IsTaskExistResultDto>> GetTaskDetailsForRequestByTaskType(ImportAuthenticationRequestDto request, List<int> taskTypeIds)
    {
        var filter = new IsTaskExistFilterDto
        {
            EntityId = request.DocumentId,
            EntityTypeId = 12384, // EEntityType.ImportAuthenticationRequest
            TaskTypeIds = taskTypeIds
        };
        var result = await tasksProxy.IsTaskExist(filter);
        return result ?? new List<IsTaskExistResultDto>();
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@VendorID", vendorId, DbType.Int32);
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForVendor(parameters);
        return result;
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var result = await DataLayer.CheckImporterOfImportAuthentication(importerId);
        return result;
    }

    public async Task<ImportAuthenticationFileDetailsDto?> GetAuthenticationRequestFileById(int fileId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@FileID", fileId, DbType.Int32);

        var file = await DataLayer.GetAuthenticationFileDetailsById(parameters);
        if (file == null)
        {
            return null;
        }

        await GetAdditionalInfoForFile(file);
        return file;
    }

    private async Task GetAdditionalInfoForFile(ImportAuthenticationFileDetailsDto file)
    {
        var decisions = await DataLayer.GetCertificateOfOriginsDecisions() ?? new List<CertificateOfOriginsDecisionDto>();
        file.FileStatuses = await DataLayer.GetAuthenticationFileStatuses() ?? new List<CertificateOfOriginsAuthenticationFileStatusDto>();

        foreach (var request in file.Requests)
        {
            request.Decisions = decisions;

            var collaterals = await collateralProxy.GetCollateralRequest(
                12384 /* EEntityType.ImportAuthenticationRequest */, request.DocumentId, null);
            if (collaterals != null && collaterals.Count > 0)
            {
                request.Collaterals = collaterals;
            }
        }

        var tasks = await GetTaskDetailsForFileByTaskType(file, new List<int>
        {
            339, // ETaskType.ReminderNotice6Months
            340, // ETaskType.ReminderNotice10Months
            408, // ETaskType.HandleAuthenticationRequestFile
            404  // ETaskType.SendReminderForImporter
        });

        var userUtil = Resolve<IUserUtil>();
        var currentUserId = (await userUtil.GetUserId(RequestMetadata)).GetValueOrDefault();
        file.IsCurrentUserHandleFile = tasks.Any(t => t.UserId == currentUserId);
    }

    private async Task<List<IsTaskExistResultDto>> GetTaskDetailsForFileByTaskType(ImportAuthenticationFileDetailsDto file, List<int> taskTypeIds)
    {
        var filter = new IsTaskExistFilterDto
        {
            EntityId = file.Id,
            EntityTypeId = 12385, // EEntityType.AuthenticationRequestFile
            TaskTypeIds = taskTypeIds
        };
        var result = await tasksProxy.IsTaskExist(filter);
        return result ?? new List<IsTaskExistResultDto>();
    }
}
