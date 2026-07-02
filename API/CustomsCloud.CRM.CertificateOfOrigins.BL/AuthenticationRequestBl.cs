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
        var result = await DataLayer.GetAuthenticationRequestByFilter(filter);
        await FillAuthenticationRequestNames(result);
        return result;
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

        // TODO(blocking): legacy UserUtil.Current.ID — IRequestMetadata exposes CertificateId/Username but not a
        // numeric UserId; confirm the current-user-id source with the team (same open point as in PreRulings).
        var currentUserId = 1;
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
        var file = await DataLayer.GetAuthenticationFileById(fileId);
        if (file == null)
        {
            return null;
        }

        file.CustomerIdList = new List<int>(); // legacy: initialized empty, never populated in this path

        var requests = await DataLayer.GetRequestsByAuthenticationFileId(fileId);
        file.Requests = requests;
        file.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [1055] = requests.Select(r => r.LeadDocumentId).ToList() // EntityType.ImportDeclaration = 1055
        };

        var requestDocumentIds = requests.Select(r => r.DocumentId).ToList();
        if (requestDocumentIds.Count > 0)
        {
            var documents = await documentsProxy.GetDocumentsByIds(requestDocumentIds);
            var itemDetails = await DataLayer.GetItemDetailsByRequestIds(requestDocumentIds);
            foreach (var request in requests)
            {
                request.Document = documents?.FirstOrDefault(d => d.Id == request.DocumentId);
                request.ItemDetails = itemDetails.Where(i => i.ImportAuthenticationRequestId == request.DocumentId).ToList();
                request.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
                {
                    [1055] = new List<int> { request.LeadDocumentId }
                };

                // TODO (blocking): legacy also returned LeadDocumentSubmissionDate (CRP.DealFile_LeadDocumentSubmissionData)
                // and IsSendReminderForImporterTaskExists (open task 404) per request — DealFile has no microservice;
                // the reminder-task flag can be filled via ITasksProxy.IsTaskExist once semantics are confirmed.
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

        // TODO(blocking): legacy UserUtil.Current.ID — current-user-id source pending (see CreateNewAuthenticationFile)
        var currentUserId = 1;
        file.IsCurrentUserHandleFile = tasks != null && tasks.Any(t => t.UserId == currentUserId);
        return file;
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
