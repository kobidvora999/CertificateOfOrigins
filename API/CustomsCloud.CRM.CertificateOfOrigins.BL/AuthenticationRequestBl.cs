using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Utils.Events;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Lookup.Entities;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Users;
using Dapper;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICollateralProxy collateralProxy,
    ITasksProxy tasksProxy,
    ICustomerProxy customerProxy,
    IVendorProxy vendorProxy,
    IDocumentsProxy documentsProxy,
    ILookupUtil lookupUtil,
    IParametersUtil parametersUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginDal>(serviceProvider)
{
    #region LEGACY_WCF

    // public List<DocumentDTO> GetEntityDocuments(CertificateOfOriginsImportAuthenticationRequest importAuthenticationRequest)
    // {
    //     var requestedDocuments = GetQuery<CertificateOfOriginsImportAuthenticationRequest>()
    //         .Where(cr => cr.LeadDocumentID == request.LeadDocumentID).ToList();
    //     var documentTypeIDs = Configuration.GetConfig<string>("CertificateOfOriginsDocumentsFilter"); // CSV
    //     var entityDocuments = Container.Resolve<IDocumentsExternalProxy>()
    //         .GetDocumentsByEntitySync(request.LeadDocumentID, EEntityType.ImportDeclaration);
    //     ... filters: exclude already-requested IDs, keep configured TypeIDs, exclude ID==0,
    //         exclude documents already used by a different lead document ...
    //     foreach: map to DocumentDTO { Notes = "{ID} {Title} {TypeName}", StringDynamicParams = entity.Notes,
    //              TypeName = SystemTablesUtil.GetCodeById<DocumentType>(TypeID).Name, OtherRelatedEntities }
    // }
    // The Documents microservice response already carries TypeName + OtherRelatedEntities, so the
    // DocumentType lookup is not needed in the new implementation.
    #endregion
    public async Task<List<DocumentDto>> GetEntityDocuments(ImportAuthenticationRequestDto importAuthenticationRequest)
    {
        var requestedDocumentIds = await DataLayer.GetRequestedDocumentIdsByLeadDocumentId(importAuthenticationRequest.LeadDocumentId);

        var documentTypeIds = await parametersUtil.Get<string>("CertificateOfOriginsDocumentsFilter");
        var documentFilter = documentTypeIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Convert.ToInt32(s))
            .ToList();

        var entityDocuments = await documentsProxy.GetDocumentsByEntity(
            importAuthenticationRequest.LeadDocumentId,
            1055); // EntityType.ImportDeclaration = 1055 (CustomsCloud.Infrastructure.Documents.Model.DTO.EntityType)
        entityDocuments = entityDocuments?.Where(entDoc => !requestedDocumentIds.Contains(entDoc.Id)).ToList();
        if (entityDocuments == null)
        {
            return new List<DocumentDto>();
        }

        var entityDocumentsFilteredList = entityDocuments.Where(d => documentFilter.Any(f => f == d.TypeId)).ToList();
        if (requestedDocumentIds.Count > 0)
        {
            entityDocumentsFilteredList = entityDocumentsFilteredList
                .Where(d => requestedDocumentIds.All(requestedId => requestedId != d.Id) && d.Id != 0)
                .ToList();
        }

        var ids = entityDocumentsFilteredList.Select(e => e.Id).ToList();
        var documentIdsByOtherLeadDocumentId = await DataLayer.GetDocumentIdsUsedByOtherLeadDocuments(ids, importAuthenticationRequest.LeadDocumentId);
        if (documentIdsByOtherLeadDocumentId.Count > 0)
        {
            entityDocumentsFilteredList = entityDocumentsFilteredList
                .Where(d => !documentIdsByOtherLeadDocumentId.Contains(d.Id))
                .ToList();
        }

        if (entityDocumentsFilteredList.Count == 0)
        {
            return new List<DocumentDto>();
        }

        var docs = new List<DocumentDto>();
        foreach (var entityDocument in entityDocumentsFilteredList)
        {
            var doc = new DocumentDto
            {
                Id = entityDocument.Id,
                IsIncoming = entityDocument.IsIncoming,
                CreateDate = entityDocument.CreateDate,
                Title = entityDocument.Title,
                TypeName = entityDocument.TypeName,
                IsAccepted = entityDocument.IsAccepted,
                IsRequired = entityDocument.IsRequired,
                Notes = entityDocument.Id + " " + entityDocument.Title + " " + entityDocument.TypeName,
                ExternalId = entityDocument.ExternalId,
                TypeId = entityDocument.TypeId,
                StringDynamicParams = entityDocument.Notes,
                OtherRelatedEntities = entityDocument.OtherRelatedEntities ?? new List<EntityDocumentDto>()
            };
            docs.Add(doc);
        }

        return docs;
    }

    #region LEGACY_WCF

    // public List<GetImportAuthenticationRequestResult> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilter filter)
    // {
    //     var result = _uow.Repository.ExecuteFunction<GetImportAuthenticationRequestResult>(filter).ToList();
    //     return result;
    // }
    // The legacy SP [CRM].[usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter] assembled a
    // dynamic WHERE over CRM.CertificateOfOrigins_ImportAuthenticationRequest with optional predicates,
    // joined local tables (PrefernceDocumentType, ImportAuthenticationFileDetails) and remote-replica
    // tables for display names (Country, OrganizationUnit, Vendor, Customer, DealFile_LeadDocument),
    // TOP(ufn_GetMaxRows()) ordered by CreateDate DESC. Migrated to LINQ; display names are enriched via
    // ILookupUtil (Country, OrganizationUnit) and the Customers/Vendors microservices because those
    // tables are not replicated to this service's DB.
    #endregion
    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetAuthenticationRequestByFilter(parameters);
        await FillAuthenticationRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrefernceDocumentType", filter.PrefernceDocumentType, DbType.Int32);
        parameters.Add("@GoodsOrigionCountry", filter.GoodsOrigionCountry, DbType.Int32);
        parameters.Add("@IssuingCountry", filter.IssuingCountry, DbType.Int32);
        parameters.Add("@ImportCountry", filter.ImportCountry, DbType.Int32);
        parameters.Add("@FromRequestDate", filter.FromRequestDate?.DateTime, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate?.DateTime, DbType.DateTime);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@RequestReason", filter.RequestReason, DbType.Int32);
        parameters.Add("@LeadDocumentID", filter.LeadDocumentId, DbType.Int32);
        parameters.Add("@ImporterID", filter.ImporterId, DbType.Int32);
        parameters.Add("@VendorID", filter.VendorId, DbType.Int32);
        parameters.Add("@DecisionID", filter.DecisionId, DbType.Int32);
        parameters.Add("@CustomerID", filter.CustomerId, DbType.Int32);
        parameters.Add("@DocumentID", filter.DocumentId, DbType.Int32);
        parameters.Add("@InvoiceNumber", filter.InvoiceNumber, DbType.String);
        parameters.Add("@DocumentNumber", filter.DocumentNumber, DbType.String);
        parameters.Add("@AuthenticationFileID", filter.AuthenticationFileId, DbType.Int32);
        parameters.Add("@CreateUserID", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    #region LEGACY_WCF

    // public CertificateOfOriginsImportAuthenticationFileDetails CreateNewAuthenticationFile(List<GetImportAuthenticationRequestResult> importAuthenticationRequests)
    // {
    //     guard null/empty → null;
    //     duplicate check: first request with AuthenticationFileID != null → throw InfException(EMessages.FileExistForRequest /*14070*/, [DocumentID, fileID]);
    //     file = new CertificateOfOriginsImportAuthenticationFileDetails { State=1, Status=WaitingForSendingLetter(1),
    //            RequestCountryID = first.IssuingCountryIDNum ?? 0, UserID/CreateUserID/UpdateUserID = UserUtil.Current.ID,
    //            PostalAdress="gg", DeliveryMethodID=1, EmailAdress=first.ResponseNameEmail, ReminderMethodID=1,
    //            UserNameIssuingLetter="ss", CreateDate/UpdateDate=Now, CustomerID = first.CustomerID ?? 1 /*transient*/ };
    //     file.CustomerIDList = all non-null CustomerIDs; foreach request → RaiseEvent NewDecisionBeforeAssociation;
    //     file.OrganizationUnitID = first.OrganizationUnitIDNum.Value /*transient, unguarded*/;
    //     Save + Commit; ExecuteFunction [CRM].[usp_CertificateOfOrigins_UpdateImportAuthenticationRequest](TVP ids, fileID)
    //       — plain bulk UPDATE ... SET AuthenticationFileID WHERE DocumentID IN ids AND AuthenticationFileID IS NULL
    //       (converted to ExecuteUpdateAsync in the DAL per module guidelines); Commit;
    //     RaiseEvent NewAuthenticationRequestFile; return file;
    // }
    #endregion
    public async Task<ImportAuthenticationFileDetailsDto?> CreateNewAuthenticationFile(List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        if (importAuthenticationRequests == null || importAuthenticationRequests.Count == 0)
        {
            return null;
        }

        var documentIds = importAuthenticationRequests.Where(r => r.DocumentId.HasValue).Select(r => r.DocumentId!.Value).ToList();
        var requestWithFile = await DataLayer.GetFirstRequestWithAuthenticationFile(documentIds);
        if (requestWithFile != null)
        {
            // legacy: InfException(EMessages.FileExistForRequest /*14070*/, [DocumentID, fileID])
            throw new RestValidationException(requestWithFile.DocumentId.ToString(),
                $"Authentication file {requestWithFile.AuthenticationFileId} already exists for request {requestWithFile.DocumentId}", "400");
        }

        var firstRequest = importAuthenticationRequests.First();

        var userUtil = Resolve<IUserUtil>();
        var currentUserId = (await userUtil.GetUserId(RequestMetadata)).GetValueOrDefault();
        var file = new ImportAuthenticationFileDetails
        {
            State = 1,
            AuthenticationFileStatusId = 1, // EAuthenticationFileStatus.WaitingForSendingLetter
            RequestCountryId = firstRequest.IssuingCountryIdNum ?? 0,
            UserId = currentUserId,
            PostalAdress = "gg", // legacy hardcoded placeholder — preserved for parity
            DeliveryMethodId = 1,
            EmailAdress = firstRequest.ResponseNameEmail,
            ReminderMethodId = 1,
            UserNameIssuingLetter = "ss", // legacy hardcoded placeholder — preserved for parity
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            UpdateUserId = currentUserId,
            CreateUserId = currentUserId
        };

        var customerIdList = importAuthenticationRequests.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value).ToList();

        var eventUtil = Resolve<IEventUtil>();
        foreach (var request in importAuthenticationRequests)
        {
            await RaiseEventNewDecisionBeforeAssociation(eventUtil, request);
        }

        // legacy: file.OrganizationUnitID = first.OrganizationUnitIDNum.Value — transient field, unguarded (throws on null)
        var organizationUnitId = firstRequest.OrganizationUnitIdNum!.Value;

        file = await DataLayer.CreateAuthenticationFile(file);
        await DataLayer.UpdateRequestsAuthenticationFileId(documentIds, file.Id);

        var fileEventRequest = eventUtil.CreatBuilder()
            .WithEventType(1517) // EEventType.NewAuthenticationRequestFile
            .WithEntityId(file.Id)
            .WithEntityType(12385) // EntityType.AuthenticationRequestFile (CustomsCloud.Infrastructure.Documents.Model.DTO.EntityType)
            .WithTitle(file.Id.ToString())
            .WithOrganizationUnitId(organizationUnitId)
            .WithAdditionalInfo(file.Id.ToString())
            .Build();
        await eventUtil.RaiseEvent(fileEventRequest); // opens task type HandleAuthenticationRequestFile

        var result = MapAuthenticationFileToDto(file, organizationUnitId, customerIdList, firstRequest.CustomerId ?? 1);
        return result;
    }

    #region LEGACY_WCF

    // public CertificateOfOriginsImportAuthenticationFileDetails GetAuthenticationRequestFileByID(int fileId)
    // {
    //     ExecuteResultSetFunction([CRM].[usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests], @FileID)
    //     — 4 result sets: file header; requests by AuthenticationFileID (+LeadDocumentSubmissionDate from
    //       CRP.DealFile_LeadDocumentSubmissionData, +IsSendReminderForImporterTaskExists from open task 404);
    //       documents (Infrastructure.Docs_Document by request DocumentIDs); item details (CRM.CertificateOfOrigins_ItemDetails).
    //     MaterializeForGetFile assembled the graph, set EntityTypeAndIDsToSearch[ImportDeclaration],
    //     CustomerID==0 → -1. GetAdditionalInfoForFile then loaded the Decision/FileStatus lookup tables,
    //     fetched collaterals per request (Collateral adapter) and computed IsCurrentUserHandleFile from
    //     open file tasks (Tasks adapter, task types 339/340/408/404) vs UserUtil.Current.ID.
    //     Migrated: local result sets via LINQ; documents via Documents proxy; tasks/collaterals via proxies.
    // }
    #endregion
    public async Task<ImportAuthenticationFileDetailsDto?> GetAuthenticationRequestFileByID(int fileId)
    {
        var (file, requests, itemDetails) = await DataLayer.GetAuthenticationFileDetailsAndRequests(fileId);
        if (file == null)
        {
            return null;
        }

        file.CustomerIdList = new List<int>(); // legacy: initialized empty, never populated in this path

        file.Requests = requests;
        file.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [1055] = requests.Select(r => r.LeadDocumentId).ToList() // EntityType.ImportDeclaration = 1055
        };

        var requestDocumentIds = requests.Select(r => r.DocumentId).ToList();
        if (requestDocumentIds.Count > 0)
        {
            var documents = await documentsProxy.GetDocumentsByIds(requestDocumentIds);
            foreach (var request in requests)
            {
                request.Document = documents?.FirstOrDefault(d => d.Id == request.DocumentId);
                request.ItemDetails = itemDetails.Where(i => i.ImportAuthenticationRequestId == request.DocumentId).ToList();
                request.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
                {
                    [1055] = new List<int> { request.LeadDocumentId }
                };

                // TODO (blocking): LeadDocumentSubmissionDate returns NULL from the SP (CRP.DealFile_* not replicated —
                // no DealFile microservice) and IsSendReminderForImporterTaskExists returns 0 (Infrastructure.Tasks_Task
                // not replicated) — the reminder-task flag can be filled via ITasksProxy.IsTaskExist once semantics are confirmed.
            }
        }

        if (file.CustomerId == 0)
        {
            file.CustomerId = -1;
        }

        var decisions = await DataLayer.GetAllDecisions();
        file.FileStatuses = await DataLayer.GetAllAuthenticationFileStatuses();
        foreach (var request in requests)
        {
            request.Decisions = decisions;

            var collateralList = await collateralProxy.GetCollateralRequest(12384, request.DocumentId, null); // EntityType.ImportAuthenticationRequest = 12384
            if (collateralList != null && collateralList.Count > 0)
            {
                request.Collaterals = collateralList;
            }
        }

        var tasks = await tasksProxy.IsTaskExist(new IsTaskExistFilterDto
        {
            EntityId = file.Id,
            EntityTypeId = 12385, // EntityType.AuthenticationRequestFile
            TaskTypeIds = new List<int> { 339, 340, 408, 404 } // ETaskType: ReminderNotice6Months, ReminderNotice10Months, HandleAuthenticationRequestFile, SendReminderForImporter
        });

        var userUtil = Resolve<IUserUtil>();
        var currentUserId = (await userUtil.GetUserId(RequestMetadata)).GetValueOrDefault();
        file.IsCurrentUserHandleFile = tasks != null && tasks.Any(t => t.UserId == currentUserId);
        return file;
    }

    #region LEGACY_WCF

    // public CertificateOfOriginsImportAuthenticationRequest GetAuthenticationRequestByID(int documentId)
    // {
    //     ExecuteResultSetFunction([CRM].[usp_CertificateOfOrigins_CertificateOfOrigins_GetImportAuthenticationRequestById], @DocumentId)
    //     — 3 result sets: request row (+LeadDocumentSubmissionDate from CRP.DealFile_LeadDocumentSubmissionData);
    //       item details (CRM.CertificateOfOrigins_ItemDetails); document (Infrastructure.Docs_Document, FileUrl = DocumentType name).
    //     GetAdditionalInfoForRequest: all decisions; collaterals via Collateral adapter; tasks 406/404/407 via Tasks
    //     adapter → IsCurrentUserHandleRequest/IsCurrentUserHasOpenTask vs UserUtil.Current.ID;
    //     EntityTypeAndIDsToSearch[ImportDeclaration] = [LeadDocumentID].
    //     Then: AdditionalRequestsForSearchInDays = GetConfig<int>; IsVendorByIssuingCountryID via
    //     SystemTablesUtil.GetIdByCode<CertificateOfOriginSupplierDeliveryCountryConfig>("ConutryID", countryId).
    //     Legacy threw NRE when the request was not found — the migrated version returns null (per master precedent).
    // }
    #endregion
    public async Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(int documentId)
    {
        var importAuthenticationRequest = await DataLayer.GetAuthenticationRequestById(documentId);
        if (importAuthenticationRequest == null)
        {
            return null;
        }

        // ItemDetails arrive from the SP's 2nd result set (attached in the DAL)
        var documents = await documentsProxy.GetDocumentsByIds(new List<int> { documentId });
        importAuthenticationRequest.Document = documents?.FirstOrDefault(d => d.Id == documentId);
        if (importAuthenticationRequest.Document != null)
        {
            // legacy: Document.FileUrl holds the DocumentType display name (field reuse preserved for parity)
            importAuthenticationRequest.Document.FileUrl = importAuthenticationRequest.Document.TypeName;
        }

        // TODO (blocking): legacy also returned LeadDocumentSubmissionDate from CRP.DealFile_LeadDocumentSubmissionData —
        // DealFile has no microservice; the data source must be decided with the team.
        await GetAdditionalInfoForRequest(importAuthenticationRequest);

        importAuthenticationRequest.AdditionalRequestsForSearchInDays =
            await parametersUtil.Get<int>("AdditionalRequestsForSearchInDays");
        importAuthenticationRequest.IsVendorByIssuingCountryId =
            await DataLayer.IsVendorCountry(importAuthenticationRequest.IssuingCountryId);

        return importAuthenticationRequest;
    }

    private async Task GetAdditionalInfoForRequest(ImportAuthenticationRequestDto request)
    {
        request.Decisions = await DataLayer.GetAllDecisions();

        var collaterals = await collateralProxy.GetCollateralRequest(12384, request.DocumentId, null); // EntityType.ImportAuthenticationRequest
        if (collaterals != null && collaterals.Count > 0)
        {
            request.Collaterals = collaterals;
        }

        var tasks = await tasksProxy.IsTaskExist(new IsTaskExistFilterDto
        {
            EntityId = request.DocumentId,
            EntityTypeId = 12384, // EntityType.ImportAuthenticationRequest
            TaskTypeIds = new List<int> { 406, 404, 407 } // ETaskType: SetDecisionBeforeAssociation, SendReminderForImporter, HandleRejectedAuthenticationRequest
        }) ?? new List<IsTaskExistResultDto>();

        var userUtil = Resolve<IUserUtil>();
        var currentUserId = (await userUtil.GetUserId(RequestMetadata)).GetValueOrDefault();
        request.IsCurrentUserHandleRequest = tasks.Any(t => t.UserId == currentUserId);
        request.IsCurrentUserHasOpenTask = tasks.Any(t => t.UserId == currentUserId && t.IsTaskInProgress);

        request.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [1055] = new List<int> { request.LeadDocumentId } // EntityType.ImportDeclaration
        };
    }

    #region LEGACY_WCF

    // CheckIfExistsAdditionalRequestsForImporter(request) — SP [CRM].[usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter]:
    //   IsVendor = exists row in cf_SupplierDeliveryCountryConfig for the country;
    //   days = GlobalParam 'AdditionalRequestsForSearchInDays';
    //   exists file+request where ImporterID matches, (VendorId | CustomerID) matches by IsVendor,
    //   and F.LastDelivery >= now - days. Converted to LINQ per module guidelines (flattened params per master precedent).
    // CheckIfExistsAdditionalRequestsForVendor(vendorId) — SP: COUNT(requests where VendorId, CreateDate >= now-3y) > 1.
    // CheckImporterOfImportAuthentication(importerId) — prohibited-importers check (master precedent): prohibited → null.
    #endregion
    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId)
    {
        var daysForLastDelivery = await parametersUtil.Get<int>("AdditionalRequestsForSearchInDays");
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId, daysForLastDelivery);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return result;
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var result = await DataLayer.CheckImporterOfImportAuthentication(importerId);
        return result;
    }

    #region LEGACY_WCF

    // HandleImportAuthenticationRequestDeliveryForImporterSent / ...ReminderForImporterSent →
    //   HandleReminderOrDeliveryRequestSentToImporter(request, eventType, decision):
    //     set DecisionID + LastDeliveryForImporter/UpdateDate = Today; UpdateFileAfterDelivery(file nav);
    //     Save + Commit; RaiseEvent(eventType, ImportAuthenticationRequest, related file when exists).
    // HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(file, isDelivery):
    //     reminder → status = AuthenticationRequestReminderWasSend(3); UpdateFileAfterDelivery(file).
    // UpdateFileAfterDelivery — status/delivery-method state machine:
    //     WaitingForSendingLetter(1) → status AuthenticationRequestWasSend(2) + PostedMailing(2);
    //     AuthenticationRequestWasSend(2): PostedMailing(2)/SentByEmailRequest(3)→FirstRemindSent(4); FirstRemindSent(4)→SecondRemindSent(5);
    //     AuthenticationRequestReminderWasSend(3): FirstRemindSent(4)→SecondRemindSent(5);
    //     LastDelivery/UpdateDate = Today; children UpdateDate = Now; Save + Commit.
    // HandleSendRemindDeliverNotification (BL CloseReminderTask): RaiseEvent CloseTaskReminderNotice3Months(1745)
    //     on the file (+ related file entity); returns true.
    // ChangeStatusAfterDeliverySent: RaiseEvent CloseAllTaskForImportAuthenticationRequestFile(1525) on the file.
    // In the WCF flow the file arrived as the request's nav property (client-tracked); in .NET 10 it is loaded
    // from the DB by AuthenticationFileId.
    #endregion
    public async Task<ImportAuthenticationRequestDto> HandleImportAuthenticationRequestDeliveryForImporterSent(ImportAuthenticationRequestDto authenticationRequest)
    {
        var result = await HandleReminderOrDeliveryRequestSentToImporter(authenticationRequest,
            1511, // EEventType.NewDeliveryForImporterSent
            8); // EAuthenticationRequestDecision.LetterForImporterWasSent
        return result;
    }

    public async Task<ImportAuthenticationRequestDto> HandleImportAuthenticationRequestDeliveryReminderForImporterSent(ImportAuthenticationRequestDto authenticationRequest)
    {
        var result = await HandleReminderOrDeliveryRequestSentToImporter(authenticationRequest,
            1512, // EEventType.NewDeliveryReminderForImporterSent
            9); // EAuthenticationRequestDecision.ReminderForImporterWasSent
        return result;
    }

    public async Task<ImportAuthenticationFileDetailsDto> HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(ImportAuthenticationFileDetailsDto authenticationFile, bool isDelivery)
    {
        if (!isDelivery)
        {
            authenticationFile.AuthenticationFileStatusId = 3; // EAuthenticationFileStatus.AuthenticationRequestReminderWasSend
        }

        await UpdateFileAfterDelivery(authenticationFile);
        return authenticationFile;
    }

    public async Task<bool> HandleSendRemindDeliverNotification(ImportAuthenticationFileDetailsDto file)
    {
        var eventUtil = Resolve<IEventUtil>();
        var eventRequest = eventUtil.CreatBuilder()
            .WithEventType(1745) // EEventType.CloseTaskReminderNotice3Months
            .WithEntityId(file.Id)
            .WithEntityType(12385) // EntityType.AuthenticationRequestFile
            .WithTitle(file.Id.ToString())
            .AddRelatedEntity(file.Id, (CustomsCloud.InfrastructureCore.Utils.Shared.EntityType)12385)
            .Build();
        await eventUtil.RaiseEvent(eventRequest);
        return true;
    }

    public async Task<ImportAuthenticationFileDetailsDto> ChangeStatusAfterDeliverySent(ImportAuthenticationFileDetailsDto authenticationRequestFile)
    {
        var eventUtil = Resolve<IEventUtil>();
        var eventRequest = eventUtil.CreatBuilder()
            .WithEventType(1525) // EEventType.CloseAllTaskForImportAuthenticationRequestFile
            .WithEntityId(authenticationRequestFile.Id)
            .WithEntityType(12385) // EntityType.AuthenticationRequestFile
            .WithTitle(authenticationRequestFile.Id.ToString())
            .WithOrganizationUnitId(authenticationRequestFile.OrganizationUnitId)
            .Build();
        await eventUtil.RaiseEvent(eventRequest);
        return authenticationRequestFile;
    }

    #region LEGACY_WCF

    // External HandleAuthenticationRequestDeliverySent(raiseEventArgs): find the related entity of type
    // AuthenticationRequestFile; missing → false; load the file by id; null → false; otherwise true.
    // (The actual update — UpdateFileAfterDelivery — is commented out in the WCF source; preserved as-is.)
    #endregion
    public async Task<bool> HandleAuthenticationRequestDeliverySent(RaiseEventArgsDto raiseEventArgs)
    {
        if (raiseEventArgs.RelatedEntities == null || raiseEventArgs.RelatedEntities.Count == 0)
        {
            return false;
        }

        var authenticationRequestFileEntity = raiseEventArgs.RelatedEntities.SingleOrDefault(
            e => e.EntityType == 12385 || e.TypeId == 12385); // EntityType.AuthenticationRequestFile
        if (authenticationRequestFileEntity == null)
        {
            return false;
        }

        var authenticationRequestFile = await GetAuthenticationRequestFileByID(authenticationRequestFileEntity.Id);
        return authenticationRequestFile != null;
    }

    private async Task<ImportAuthenticationRequestDto> HandleReminderOrDeliveryRequestSentToImporter(ImportAuthenticationRequestDto authenticationRequest, int eventType, int decision)
    {
        authenticationRequest.DecisionId = decision;
        authenticationRequest.LastDeliveryForImporter = DateTime.Today;
        authenticationRequest.UpdateDate = DateTime.Today;

        if (authenticationRequest.AuthenticationFileId.HasValue)
        {
            var file = await DataLayer.GetAuthenticationFileById(authenticationRequest.AuthenticationFileId.Value);
            if (file != null)
            {
                await UpdateFileAfterDelivery(file);
            }
        }

        await DataLayer.SaveImportAuthenticationRequest(authenticationRequest);

        var eventUtil = Resolve<IEventUtil>();
        var builder = eventUtil.CreatBuilder()
            .WithEventType(eventType)
            .WithEntityId(authenticationRequest.DocumentId)
            .WithEntityType(12384) // EntityType.ImportAuthenticationRequest
            .WithTitle(authenticationRequest.DocumentId.ToString())
            .WithOrganizationUnitId(authenticationRequest.OrganizationUnitId);
        if (authenticationRequest.AuthenticationFileId != null)
        {
            builder = builder.AddRelatedEntity(authenticationRequest.AuthenticationFileId ?? 0, (CustomsCloud.InfrastructureCore.Utils.Shared.EntityType)12385); // EntityType.AuthenticationRequestFile
        }

        var eventRequest = builder.Build();
        await eventUtil.RaiseEvent(eventRequest);

        return authenticationRequest;
    }

    private async Task UpdateFileAfterDelivery(ImportAuthenticationFileDetailsDto file)
    {
        if (file.AuthenticationFileStatusId == 1) // WaitingForSendingLetter
        {
            file.AuthenticationFileStatusId = 2; // AuthenticationRequestWasSend
            file.DeliveryMethodId = 2; // EDeliveryMethod.PostedMailing
        }
        else if (file.AuthenticationFileStatusId == 2) // AuthenticationRequestWasSend
        {
            if (file.DeliveryMethodId == 2 || file.DeliveryMethodId == 3) // PostedMailing / SentByEmailRequest
            {
                file.DeliveryMethodId = 4; // FirstRemindSent
            }
            else if (file.DeliveryMethodId == 4) // FirstRemindSent
            {
                file.DeliveryMethodId = 5; // SecondRemindSent
            }
        }
        else if (file.AuthenticationFileStatusId == 3) // AuthenticationRequestReminderWasSend
        {
            if (file.DeliveryMethodId == 4) // FirstRemindSent
            {
                file.DeliveryMethodId = 5; // SecondRemindSent
            }
        }

        file.LastDelivery = DateTime.Today;
        file.UpdateDate = DateTime.Today;

        await DataLayer.SaveAuthenticationFileDetails(file);
        await DataLayer.TouchRequestsUpdateDateByFileId(file.Id, DateTime.Now);
    }

    private static async Task RaiseEventNewDecisionBeforeAssociation(IEventUtil eventUtil, GetImportAuthenticationRequestResultDto request)
    {
        var eventRequest = eventUtil.CreatBuilder()
            .WithEventType(1515) // EEventType.NewDecisionBeforeAssociation
            .WithEntityId((int)request.DocumentId!)
            .WithEntityType(12384) // EntityType.ImportAuthenticationRequest (CustomsCloud.Infrastructure.Documents.Model.DTO.EntityType)
            .WithTitle(request.DocumentId.ToString()!)
            .WithAdditionalInfo(request.DocumentId.ToString()!)
            .Build();
        await eventUtil.RaiseEvent(eventRequest); // closes task type SetDecisionBeforeAssociation
    }

    private static ImportAuthenticationFileDetailsDto MapAuthenticationFileToDto(ImportAuthenticationFileDetails file, int organizationUnitId, List<int> customerIdList, int customerId)
    {
        return new ImportAuthenticationFileDetailsDto
        {
            Id = file.Id,
            State = file.State,
            CreateDate = file.CreateDate,
            CreateUserId = file.CreateUserId,
            UpdateDate = file.UpdateDate,
            UpdateUserId = file.UpdateUserId,
            AuthenticationFileStatusId = file.AuthenticationFileStatusId,
            Notes = file.Notes,
            PostalAdress = file.PostalAdress,
            DeliveryMethodId = file.DeliveryMethodId,
            EmailAdress = file.EmailAdress,
            ReminderMethodId = file.ReminderMethodId,
            RequestCountryId = file.RequestCountryId,
            UserId = file.UserId,
            UserNameIssuingLetter = file.UserNameIssuingLetter,
            LastDelivery = file.LastDelivery,
            ImporterContactingReasonId = file.ImporterContactingReasonId,
            FirstProvideContactDate = file.FirstProvideContactDate,
            CustomerIdList = customerIdList,
            CustomerId = customerId,
            OrganizationUnitId = organizationUnitId
        };
    }

    private async Task FillAuthenticationRequestNames(List<GetImportAuthenticationRequestResultDto> requests)
    {
        if (requests.Count == 0)
        {
            return;
        }

        var countries = await lookupUtil.All<Country>();
        var organizationUnits = await lookupUtil.All<OrganizationUnit>();

        var customerIds = requests.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value).Distinct().ToList();
        var customers = customerIds.Count > 0 ? await customerProxy.GetCustomersByIds(customerIds) : null;

        var vendorIds = requests.Where(r => r.VendorId.HasValue).Select(r => r.VendorId!.Value).Distinct().ToList();
        var vendors = vendorIds.Count > 0 ? await vendorProxy.GetVendorsByIds(vendorIds) : null;

        foreach (var request in requests)
        {
            request.IssuingCountryId = countries?.FirstOrDefault(c => c.Id == request.IssuingCountryIdNum)?.Name;
            request.OrganizationUnitId = organizationUnits?.FirstOrDefault(o => o.Id == request.OrganizationUnitIdNum)?.Name;
            request.ImporterName = customers?.FirstOrDefault(c => c.Id == request.CustomerId)?.Name;
            request.VendorName = vendors?.FirstOrDefault(v => v.Id == request.VendorId)?.Title;

            // TODO (blocking): LeadDocumentTitle came from CRP.DealFile_LeadDocument — no DealFile
            // microservice exists in CustomsMicroServices; the data source must be decided with the team.
            request.LeadDocumentTitle = null;
        }
    }
}
