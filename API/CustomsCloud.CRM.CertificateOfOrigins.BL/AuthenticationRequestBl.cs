using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Dapper;
using Lookup;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    IParametersUtil parametersUtil,
    ILookupUtil lookupUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
    // Internal WCF: GetAuthenticationRequestByID(documentId) — a single import authentication request with its item
    // lines, the decision lookup, collaterals (Collateral service), and current-user task flags (Tasks service).
    // Missing id → 404: the legacy dereferenced a null row (a latent NRE); replaced here with the repo's not-found
    // contract (developer decision 2026-08-02). The lead Document (Documents service, SP result-set #3) and
    // LeadDocumentSubmissionDate (DealFile service, dropped cross-service JOIN) are deferred — left null.
    public async Task<GetAuthenticationRequestByIdResultDto> GetAuthenticationRequestByID(int documentId)
    {
        var collateralProxy = Resolve<ICollateralProxy>();
        var tasksProxy = Resolve<ITasksProxy>();
        var documentsProxy = Resolve<IDocumentsProxy>();
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var request = await DataLayer.GetImportAuthenticationRequestById(documentId)
            ?? throw new RestNotFoundException();

        var result = MapToResultDto(request);

        // SP result-set #2: item lines.
        var itemDetails = await DataLayer.GetItemDetailsByRequestId(documentId);
        result.ItemDetails = itemDetails
            .Select(i => new AuthenticationRequestItemDetailDto
            {
                Id = i.Id,
                ImportAuthenticationRequestId = i.ImportAuthenticationRequestId,
                CustomItemId = i.CustomItemId,
            })
            .ToList();

        // Full decision lookup table (legacy GetQuery<CertificateOfOriginsDecision>().ToList()).
        var decisions = await DataLayer.GetAllDecisions();
        result.Decisions = decisions
            .Select(d => new CertificateOfOriginsDecisionDto
            {
                Id = d.Id,
                Name = d.Name,
                State = d.State,
                Description = d.Description,
                EnglishName = d.EnglishName,
                Enumeration = d.Enumeration,
                StartDate = d.StartDate,
            })
            .ToList();

        // Collaterals (Collateral microservice).
        var collaterals = await collateralProxy.GetCollateralRequest((int)EEntityType.ImportAuthenticationRequest, documentId);
        result.Collaterals = collaterals ?? [];

        // Current-user task flags (Tasks microservice) — the legacy compared each task's UserID to UserUtil.Current.ID.
        var taskTypeIds = new List<int>
        {
            (int)ETaskType.SetDecisionBeforeAssociation,
            (int)ETaskType.SendReminderForImporter,
            (int)ETaskType.HandleRejectedAuthenticationRequest,
        };
        var tasks = await tasksProxy.IsTaskExist(documentId, (int)EEntityType.ImportAuthenticationRequest, taskTypeIds) ?? [];
        var currentUserId = RequestMetadata.UserId;
        result.IsCurrentUserHandleRequest = tasks.Any(t => t.UserId == currentUserId);
        result.IsCurrentUserHasOpenTask = tasks.Any(t => t.UserId == currentUserId && t.IsTaskInProgress);

        // Related entity ids to search: the import declaration behind the lead document.
        result.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [(int)EEntityType.ImportDeclaration] = [request.LeadDocumentId],
        };

        // Config: additional-requests search window (legacy Configuration.GetConfig<int>).
        result.AdditionalRequestsForSearchInDays = await parametersUtil.Get<int>("AdditionalRequestsForSearchInDays");

        // IsVendor: the issuing country is configured as a supplier-delivery (vendor) country (legacy IsVendor helper).
        result.IsVendorByIssuingCountryId = await DataLayer.IsSupplierDeliveryCountry(request.IssuingCountryId);

        // Lead document (SP result-set #3, Infrastructure.Docs_Document) — fetched by the request's DocumentId from
        // the Documents service. The legacy set Document.FileUrl to the DocumentType name; that name is enriched here
        // onto TypeName via the shared DocumentType lookup.
        var document = await documentsProxy.GetDocumentById(documentId);
        if (document is not null)
        {
            await lookupUtil.FillName<DocumentType, DocumentDto>(
                [document],
                d => d.TypeId,
                (d, name) => d.TypeName = name);
        }

        result.Document = document;

        // Lead-document submission date (the dropped CRP.DealFile_LeadDocumentSubmissionData JOIN) — from the
        // DealFile service by LeadDocumentId.
        result.LeadDocumentSubmissionDate = await exportDealFileProxy.GetLeadDocumentSubmissionDate(request.LeadDocumentId);

        return result;
    }

    private static GetAuthenticationRequestByIdResultDto MapToResultDto(CertificateOfOriginsImportAuthenticationRequest request)
    {
        return new GetAuthenticationRequestByIdResultDto
        {
            DocumentId = request.DocumentId,
            CreateDate = request.CreateDate,
            AuthenticationFileId = request.AuthenticationFileId,
            AuthenticationRequestDate = request.AuthenticationRequestDate,
            CollateralId = request.CollateralId,
            DecisionId = request.DecisionId,
            LeadDocumentId = request.LeadDocumentId,
            DocumentIssuingDate = request.DocumentIssuingDate,
            ImportCountryId = request.ImportCountryId,
            IssuingCountryId = request.IssuingCountryId,
            Number = request.Number,
            OriginCountryId = request.OriginCountryId,
            PreferenceDocumentTypeId = request.PreferenceDocumentTypeId,
            ResponseNameEmail = request.ResponseNameEmail,
            OrganizationUnitId = request.OrganizationUnitId,
            VendorId = request.VendorId,
            VendorName = request.VendorName,
            CustomerId = request.CustomerId,
            ImporterId = request.ImporterId,
            LastDeliveryForImporter = request.LastDeliveryForImporter,
            InvoiceNumber = request.InvoiceNumber,
        };
    }

    // Internal WCF: GetAuthenticationRequestFileByID(fileId) — a single authentication file with its child requests
    // (each enriched with document, item lines, decisions, collaterals, submission date, and the SendReminderForImporter
    // task flag), the file-status lookup, and the current-user handling flag. Missing id → 404. The legacy SP embedded
    // the CRP.DealFile submission-date JOIN and the Tasks_Task existence OUTER APPLY in SQL; here they are resolved via
    // proxies (consistent with #27). CustomerId has no SP column — always -1 (legacy 0 -> -1 fix-up).
    public async Task<GetAuthenticationRequestFileByIdResultDto> GetAuthenticationRequestFileByID(int fileId)
    {
        var tasksProxy = Resolve<ITasksProxy>();

        var file = await DataLayer.GetAuthenticationFileById(fileId)
            ?? throw new RestNotFoundException();

        var requests = await DataLayer.GetRequestsByFileId(fileId);
        var requestIds = requests.Select(request => request.DocumentId).ToList();

        // Full lookup tables (legacy: the same list shared across all requests / the file).
        var decisions = (await DataLayer.GetAllDecisions())
            .Select(decision => new CertificateOfOriginsDecisionDto
            {
                Id = decision.Id,
                Name = decision.Name,
                State = decision.State,
                Description = decision.Description,
                EnglishName = decision.EnglishName,
                Enumeration = decision.Enumeration,
                StartDate = decision.StartDate,
            })
            .ToList();
        var fileStatuses = (await DataLayer.GetAllFileStatuses())
            .Select(status => new AuthenticationFileStatusDto
            {
                Id = status.Id,
                Name = status.Name,
                State = status.State,
                Description = status.Description,
                EnglishName = status.EnglishName,
                Enumeration = status.Enumeration,
                StartDate = status.StartDate,
                EndDate = status.EndDate,
                IsAutomatic = status.IsAutomatic,
            })
            .ToList();

        // Item lines for all requests (SP result-set #4, batched).
        var allItemDetails = await DataLayer.GetItemDetailsByRequestIds(requestIds);

        var result = new GetAuthenticationRequestFileByIdResultDto
        {
            Id = file.Id,
            State = file.State,
            CreateDate = file.CreateDate,
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
            CustomerId = -1,
            FileStatuses = fileStatuses,
        };

        foreach (var request in requests)
        {
            result.Requests.Add(await BuildFileRequestDto(request, decisions, allItemDetails));
        }

        // File-level related entity ids: all child requests' lead documents.
        result.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            [(int)EEntityType.ImportDeclaration] = requests.Select(request => request.LeadDocumentId).ToList(),
        };

        // File-level IsCurrentUserHandleFile — the current user (RequestMetadata.UserId) owns an open file task.
        var fileTaskTypeIds = new List<int>
        {
            (int)ETaskType.ReminderNotice6Months,
            (int)ETaskType.ReminderNotice10Months,
            (int)ETaskType.HandleAuthenticationRequestFile,
            (int)ETaskType.SendReminderForImporter,
        };
        var fileTasks = await tasksProxy.IsTaskExist(file.Id, (int)EEntityType.AuthenticationRequestFile, fileTaskTypeIds) ?? [];
        var currentUserId = RequestMetadata.UserId;
        result.IsCurrentUserHandleFile = fileTasks.Any(task => task.UserId == currentUserId);

        return result;
    }

    // Builds one enriched child-request DTO: scalars + item lines + the shared decision lookup, plus the per-request
    // document (Documents service, TypeName-enriched via DocumentType lookup), submission date (DealFile service), the
    // SendReminderForImporter task flag (Tasks service), and collaterals (Collateral service).
    private async Task<AuthenticationFileRequestDto> BuildFileRequestDto(
        CertificateOfOriginsImportAuthenticationRequest request,
        List<CertificateOfOriginsDecisionDto> decisions,
        List<CertificateOfOriginsItemDetails> allItemDetails)
    {
        var documentsProxy = Resolve<IDocumentsProxy>();
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var tasksProxy = Resolve<ITasksProxy>();
        var collateralProxy = Resolve<ICollateralProxy>();

        var requestDto = new AuthenticationFileRequestDto
        {
            DocumentId = request.DocumentId,
            CreateDate = request.CreateDate,
            AuthenticationFileId = request.AuthenticationFileId,
            AuthenticationRequestDate = request.AuthenticationRequestDate,
            DecisionId = request.DecisionId,
            LeadDocumentId = request.LeadDocumentId,
            DocumentIssuingDate = request.DocumentIssuingDate,
            ImportCountryId = request.ImportCountryId,
            IssuingCountryId = request.IssuingCountryId,
            OriginCountryId = request.OriginCountryId,
            PreferenceDocumentTypeId = request.PreferenceDocumentTypeId,
            ResponseNameEmail = request.ResponseNameEmail,
            OrganizationUnitId = request.OrganizationUnitId,
            VendorId = request.VendorId,
            CustomerId = request.CustomerId,
            ImporterId = request.ImporterId,
            LastDeliveryForImporter = request.LastDeliveryForImporter,
            InvoiceNumber = request.InvoiceNumber,
            Decisions = decisions,
            ItemDetails = allItemDetails
                .Where(item => item.ImportAuthenticationRequestId == request.DocumentId)
                .Select(item => new AuthenticationRequestItemDetailDto
                {
                    Id = item.Id,
                    ImportAuthenticationRequestId = item.ImportAuthenticationRequestId,
                    CustomItemId = item.CustomItemId,
                })
                .ToList(),
            EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
            {
                [(int)EEntityType.ImportDeclaration] = [request.LeadDocumentId],
            },
        };

        // Document (Documents service, by DocumentId) + TypeName enrichment (DocumentType lookup).
        var document = await documentsProxy.GetDocumentById(request.DocumentId);
        if (document is not null)
        {
            await lookupUtil.FillName<DocumentType, DocumentDto>(
                [document],
                d => d.TypeId,
                (d, name) => d.TypeName = name);
        }

        requestDto.Document = document;

        // Lead-document submission date (DealFile service; legacy CRP.DealFile_LeadDocumentSubmissionData JOIN).
        requestDto.LeadDocumentSubmissionDate = await exportDealFileProxy.GetLeadDocumentSubmissionDate(request.LeadDocumentId);

        // IsSendReminderForImporterTaskExists — an open SendReminderForImporter (404) task on the request (legacy
        // Infrastructure.Tasks_Task OUTER APPLY).
        var reminderTasks = await tasksProxy.IsTaskExist(request.DocumentId, (int)EEntityType.ImportAuthenticationRequest, [(int)ETaskType.SendReminderForImporter]);
        requestDto.IsSendReminderForImporterTaskExists = reminderTasks is { Count: > 0 };

        // Collaterals (Collateral service).
        var collaterals = await collateralProxy.GetCollateralRequest((int)EEntityType.ImportAuthenticationRequest, request.DocumentId);
        requestDto.Collaterals = collaterals ?? [];

        return requestDto;
    }

    // External WCF: HandleAuthenticationRequestDeliverySent(raiseEventArgs) — an Events-subsystem callback fired when
    // a delivery-sent event is raised for an authentication file. As shipped it is a pure existence check: it locates
    // the AuthenticationRequestFile related entity and returns whether the file exists. The legacy status-write
    // (UpdateFileAfterDelivery) is COMMENTED OUT in the WCF source (developer-disabled — "is an event needed?"), so no
    // status change / event is performed — faithful to production (developer decision 2026-08-02). The file-existence
    // is checked via the DAL header read (null on missing) rather than GetAuthenticationRequestFileByID, to preserve
    // the legacy "not found → false" (the migrated BL read throws 404 instead) and skip the unused full enrichment.
    public async Task<bool> HandleAuthenticationRequestDeliverySent(RaiseEventArgsDto request)
    {
        if (request.RelatedEntities is null || request.RelatedEntities.Count == 0)
        {
            return false;
        }

        var fileEntity = request.RelatedEntities
            .SingleOrDefault(entity => entity.EntityType == (int)EEntityType.AuthenticationRequestFile);
        if (fileEntity is null)
        {
            return false;
        }

        var file = await DataLayer.GetAuthenticationFileById(fileEntity.Id);
        return file is not null;
    }

    // Internal WCF: CreateNewAuthenticationFile(requests) — creates a new import authentication-request file from a
    // set of requests and links them to it. Validates that none of the requests already belongs to a file (throws
    // RestValidationException / FileExistForRequest otherwise). Faithful to the WCF (developer decision 2026-07-30):
    // the file is built from the FIRST request's client-supplied values (trust-client, no DB fetch); CustomerIDList
    // is dropped (transient/unused). Raises NewDecisionBeforeAssociation per request, then NewAuthenticationRequestFile.
    public async Task<CreateNewAuthenticationFileResultDto?> CreateNewAuthenticationFile(List<GetImportAuthenticationRequestResultDto> importAuthenticationRequests)
    {
        if (importAuthenticationRequests is null || importAuthenticationRequests.Count == 0)
        {
            return null;
        }

        var documentIds = importAuthenticationRequests.Select(a => a.DocumentId ?? 0).ToList();

        // Validation: reject if any of these requests already belongs to a file.
        var existingLink = await DataLayer.GetFirstRequestAlreadyLinkedToFile(documentIds);
        if (existingLink is not null)
        {
            throw new RestValidationException(
                nameof(importAuthenticationRequests),
                string.Format(ErrorMessagesResources.FileExistForRequest, existingLink.Value.DocumentId, existingLink.Value.FileId));
        }

        var first = importAuthenticationRequests[0];
        var userId = RequestMetadata.UserId ?? 0;
        var now = DateTimeOffset.Now;

        // Build the new file from the first request (trust-client). "gg"/"ss" are the legacy placeholder literals,
        // preserved as-is (developer-confirmed, not TODOs).
        var file = new CertificateOfOriginsImportAuthenticationFileDetails
        {
            State = 1,
            AuthenticationFileStatusId = (int)EAuthenticationFileStatus.WaitingForSendingLetter,
            RequestCountryId = first.IssuingCountryIdNum ?? 0,
            UserId = userId,
            PostalAdress = "gg",
            DeliveryMethodId = 1,
            EmailAdress = first.ResponseNameEmail,
            ReminderMethodId = 1,
            UserNameIssuingLetter = "ss",
            CreateDate = now,
            UpdateDate = now,
            CreateUserId = userId,
            UpdateUserId = userId,
        };

        // Per-request event: NewDecisionBeforeAssociation (closes each request's SetDecisionBeforeAssociation task).
        // Raised before the insert, matching the legacy order.
        var eventUtil = Resolve<IEventUtil>();
        foreach (var request in importAuthenticationRequests)
        {
            var documentId = request.DocumentId ?? 0;
            var decisionEvent = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.NewDecisionBeforeAssociation)
                .WithEntityId(documentId)
                .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                .WithTitle(documentId.ToString())
                .WithAdditionalInfo(documentId.ToString())
                .Build();
            await eventUtil.RaiseEvent(decisionEvent);
        }

        // OrganizationUnitId is a transient (non-column) field on the legacy entity — used for the file event only.
        var organizationUnitId = first.OrganizationUnitIdNum ?? 0;

        // INSERT the file, then link the requests to it.
        var fileId = await DataLayer.InsertAuthenticationFile(file);
        await DataLayer.LinkRequestsToAuthenticationFile(documentIds, fileId);

        // Final event: NewAuthenticationRequestFile (opens the HandleAuthenticationRequestFile task).
        var fileEvent = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.NewAuthenticationRequestFile)
            .WithEntityId(fileId)
            .WithEntityType((int)EEntityType.AuthenticationRequestFile)
            .WithTitle(fileId.ToString())
            .WithOrganizationUnitId(organizationUnitId)
            .WithAdditionalInfo(fileId.ToString())
            .Build();
        await eventUtil.RaiseEvent(fileEvent);

        return new CreateNewAuthenticationFileResultDto
        {
            Id = fileId,
            AuthenticationFileStatusId = file.AuthenticationFileStatusId,
            OrganizationUnitId = organizationUnitId,
            RequestCountryId = file.RequestCountryId,
            CustomerId = first.CustomerId ?? 1,
            DeliveryMethodId = file.DeliveryMethodId,
            ReminderMethodId = file.ReminderMethodId,
            EmailAdress = file.EmailAdress,
            CreateDate = file.CreateDate,
        };
    }

    // Internal WCF: ChangeStatusAfterDeliverySent(fileDetails) — a pure event-raise passthrough. It raises
    // CloseAllTaskForImportAuthenticationRequestFile; the Events microservice's response handler closes the open
    // tasks for the file. No DB write here (the legacy status change happened client-side before the call). The WCF
    // took the full file-details entity but used only Id + OrganizationUnitId, so those are flattened into the DTO.
    public async Task<bool> ChangeStatusAfterDeliverySent(ChangeStatusAfterDeliverySentRequestDto request)
    {
        var eventUtil = Resolve<IEventUtil>();
        var eventRequest = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CloseAllTaskForImportAuthenticationRequestFile)
            .WithEntityId(request.Id)
            .WithEntityType((int)EEntityType.AuthenticationRequestFile)
            .WithTitle(request.Id.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .Build();
        await eventUtil.RaiseEvent(eventRequest);
        return true;
    }

    // Internal WCF: HandleSendRemindDeliverNotification(fileDetails) → BL CloseReminderTask — a pure event-raise
    // passthrough. It raises CloseTaskReminderNotice3Months; the Events microservice's response handler closes the
    // 3-month reminder-notice task for the file. No DB write. The WCF took the full file-details entity but used only
    // Id + OrganizationUnitId (Title is the file's computed Hebrew label, replicated here for parity).
    public async Task<bool> CloseReminderTask(CloseReminderTaskRequestDto request)
    {
        var eventUtil = Resolve<IEventUtil>();
        var eventRequest = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CloseTaskReminderNotice3Months)
            .WithEntityId(request.Id)
            .WithEntityType((int)EEntityType.AuthenticationRequestFile)
            .WithTitle($"  אימות מסמך מקור (יבוא) מספר פניה {request.Id}")
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .AddRelatedEntity(request.Id, (int)EEntityType.AuthenticationRequestFile)
            .Build();
        await eventUtil.RaiseEvent(eventRequest);
        return true;
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(file, isDelivery) — the vendor/
    // customs-house delivery flow. Advances the file's status + delivery method via the legacy status machine and
    // stamps the delivery dates on the file and its child requests. Faithful to the WCF (developer decision
    // 2026-07-29): the machine runs on the CLIENT-supplied current status + delivery method (no DB fetch); no event.
    public async Task<HandleDeliveryAndReminderForVendorSentResultDto> HandleImportAuthenticationRequestDeliveryAndReminderForVendorSent(HandleDeliveryAndReminderForVendorSentRequestDto request)
    {
        // A reminder (not a delivery) first flips the status to "reminder was sent" (vendor flow only).
        var initialStatus = request.IsDelivery
            ? request.AuthenticationFileStatusId
            : (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend;
        var (status, deliveryMethod) = AdvanceDeliveryStatus(initialStatus, request.DeliveryMethodId);

        await DataLayer.UpdateFileAfterDelivery(request.Id, status, deliveryMethod);

        return new HandleDeliveryAndReminderForVendorSentResultDto
        {
            Id = request.Id,
            AuthenticationFileStatusId = status,
            DeliveryMethodId = deliveryMethod,
        };
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryForImporterSent(request) — the importer delivery flow.
    // Delegates to the shared helper with NewDeliveryForImporterSent + LetterForImporterWasSent (mirrors the WCF).
    public async Task<HandleDeliveryOrReminderForImporterSentResultDto> HandleImportAuthenticationRequestDeliveryForImporterSent(HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await HandleReminderOrDeliveryRequestSentToImporter(
            request,
            (int)EEventType.NewDeliveryForImporterSent,
            (int)EAuthenticationRequestDecision.LetterForImporterWasSent);
        return result;
    }

    // Internal WCF: HandleImportAuthenticationRequestDeliveryReminderForImporterSent(request) — the importer reminder
    // flow. Same shared helper as the delivery flow, only with the reminder event + decision (mirrors the WCF).
    public async Task<HandleDeliveryOrReminderForImporterSentResultDto> HandleImportAuthenticationRequestDeliveryReminderForImporterSent(HandleDeliveryOrReminderForImporterSentRequestDto request)
    {
        var result = await HandleReminderOrDeliveryRequestSentToImporter(
            request,
            (int)EEventType.NewDeliveryReminderForImporterSent,
            (int)EAuthenticationRequestDecision.ReminderForImporterWasSent);
        return result;
    }

    // Shared importer delivery/reminder flow (legacy HandleReminderOrDeliveryRequestSentToImporter). Faithful order:
    // stamp the request's decision + date, advance the parent file's status machine (trust-client current values, no
    // DB fetch) which also touches the file's child requests' UpdateDate, then raise the event. #23 and #24 differ
    // only in the event type + decision passed in.
    private async Task<HandleDeliveryOrReminderForImporterSentResultDto> HandleReminderOrDeliveryRequestSentToImporter(
        HandleDeliveryOrReminderForImporterSentRequestDto request, int eventTypeId, int decisionId)
    {
        // 1. Stamp the request (DecisionID + LastDeliveryForImporter + UpdateDate).
        await DataLayer.UpdateRequestDecisionAfterDelivery(request.DocumentId, decisionId);

        // 2. Advance the parent file's status machine + touch its child requests (only if the request has a file).
        var (status, deliveryMethod) = AdvanceDeliveryStatus(request.AuthenticationFileStatusId, request.DeliveryMethodId);
        if (request.AuthenticationFileId.HasValue)
        {
            await DataLayer.UpdateFileAfterDelivery(request.AuthenticationFileId.Value, status, deliveryMethod);
        }

        // 3. Raise the event on the request (after the save, as in the legacy). Related entity = the file, if any.
        var eventUtil = Resolve<IEventUtil>();
        var builder = eventUtil.CreatBuilder()
            .WithEventType(eventTypeId)
            .WithEntityId(request.DocumentId)
            .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
            .WithTitle(request.DocumentId.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId);
        if (request.AuthenticationFileId.HasValue)
        {
            builder = builder.AddRelatedEntity(request.AuthenticationFileId.Value, (int)EEntityType.AuthenticationRequestFile);
        }

        await eventUtil.RaiseEvent(builder.Build());

        return new HandleDeliveryOrReminderForImporterSentResultDto
        {
            DocumentId = request.DocumentId,
            DecisionId = decisionId,
            AuthenticationFileStatusId = status,
            DeliveryMethodId = deliveryMethod,
        };
    }

    // Legacy UpdateFileAfterDelivery status machine (ported 1:1) — advances the file's status + delivery method from
    // their current values. Shared by the vendor (#22) and importer (#23/#24) flows.
    private static (int Status, int DeliveryMethod) AdvanceDeliveryStatus(int status, int deliveryMethod)
    {
        if (status == (int)EAuthenticationFileStatus.WaitingForSendingLetter)
        {
            status = (int)EAuthenticationFileStatus.AuthenticationRequestWasSend;
            deliveryMethod = (int)EDeliveryMethod.PostedMailing;
        }
        else if (status == (int)EAuthenticationFileStatus.AuthenticationRequestWasSend)
        {
            if (deliveryMethod == (int)EDeliveryMethod.PostedMailing || deliveryMethod == (int)EDeliveryMethod.SentByEmailRequest)
            {
                deliveryMethod = (int)EDeliveryMethod.FirstRemindSent;
            }
            else if (deliveryMethod == (int)EDeliveryMethod.FirstRemindSent)
            {
                deliveryMethod = (int)EDeliveryMethod.SecondRemindSent;
            }
        }
        else if (status == (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend
            && deliveryMethod == (int)EDeliveryMethod.FirstRemindSent)
        {
            deliveryMethod = (int)EDeliveryMethod.SecondRemindSent;
        }

        return (status, deliveryMethod);
    }

    // Internal WCF: GetEntityDocuments(importAuthenticationRequest) — the WCF took the full request entity but used
    // only its LeadDocumentID, so it is flattened to that scalar here (same precedent as
    // CheckIfExistsAdditionalRequestsForImporter). Returns the entity's documents (from the Documents service),
    // filtered to the allowed document types and to documents not already requested / claimed by another lead doc.
    public async Task<List<DocumentDto>> GetEntityDocuments(int leadDocumentId)
    {
        var documentsProxy = Resolve<IDocumentsProxy>();

        // DocumentIDs already registered under this lead document.
        var requestedDocumentIds = await DataLayer.GetImportAuthenticationRequestDocumentIdsByLeadDocumentId(leadDocumentId);

        // Allowed document types — CSV of TypeIDs (was Configuration.GetConfig<string>).
        var documentTypeIds = await parametersUtil.Get<string>("CertificateOfOriginsDocumentsFilter");
        var documentFilter = (documentTypeIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Convert.ToInt32(s))
            .ToList();

        // Documents attached to the lead document's import declaration (Documents microservice).
        var entityDocuments = await documentsProxy.GetDocumentsByEntity(leadDocumentId, (int)EEntityType.ImportDeclaration) ?? [];

        // Drop ones already requested, then keep only the allowed document types.
        entityDocuments = entityDocuments.Where(entDoc => !requestedDocumentIds.Contains(entDoc.Id)).ToList();
        var filteredList = entityDocuments.Where(d => documentFilter.Any(f => f == d.TypeId)).ToList();

        if (requestedDocumentIds.Count > 0)
        {
            filteredList = filteredList
                .Where(d => requestedDocumentIds.All(rid => rid != d.Id) && d.Id != 0)
                .ToList();
        }

        // Exclude documents already claimed by a different lead document.
        var ids = filteredList.Select(d => d.Id).ToList();
        var claimedByOthers = await DataLayer.GetImportAuthenticationRequestDocumentIdsClaimedByOtherLeadDocuments(ids, leadDocumentId);
        if (claimedByOthers.Count > 0)
        {
            filteredList = filteredList.Where(d => !claimedByOthers.Contains(d.Id)).ToList();
        }

        if (filteredList.Count == 0)
        {
            return [];
        }

        // TypeName — was SystemTablesUtil.GetCodeById<DocumentType>(TypeID).Name; via the shared DocumentType lookup.
        await lookupUtil.FillName<DocumentType, DocumentDto>(
            filteredList,
            d => d.TypeId,
            (d, name) => d.TypeName = name);

        // Composed Notes (legacy parity: "{Id} {Title} {TypeName}"). StringDynamicParams (raw notes) and
        // OtherRelatedEntities are already populated by the proxy.
        foreach (var doc in filteredList)
        {
            doc.Notes = $"{doc.Id} {doc.Title} {doc.TypeName}";
        }

        return filteredList;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetImportAuthenticationRequestByFilter(parameters);
        await FillAuthenticationRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(ImportAuthenticationRequestFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@PrefernceDocumentType", filter.PreferenceDocumentType, DbType.Int32);
        parameters.Add("@GoodsOrigionCountry", filter.GoodsOriginCountry, DbType.Int32);
        parameters.Add("@IssuingCountry", filter.IssuingCountry, DbType.Int32);
        parameters.Add("@ImportCountry", filter.ImportCountry, DbType.Int32);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@RequestReason", filter.RequestReason, DbType.Int32);
        parameters.Add("@leadDocumentID", filter.LeadDocumentId, DbType.Int32);
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

    private async Task FillAuthenticationRequestNames(List<GetImportAuthenticationRequestResultDto> requests)
    {
        var customerProxy = Resolve<ICustomerProxy>();
        var vendorProxy = Resolve<IVendorProxy>();

        if (requests.Count == 0)
        {
            return;
        }

        // ImporterName — the importer id arrives in CustomerId (SP: R.ImporterID AS CustomerID); Customers proxy.
        var importerIds = requests.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value).Distinct().ToList();
        if (importerIds.Count > 0)
        {
            var customers = await customerProxy.GetCustomersByIds(importerIds);
            if (customers != null)
            {
                var customersById = customers.ToDictionary(c => c.Id);
                foreach (var request in requests)
                {
                    if (request.CustomerId.HasValue && customersById.TryGetValue(request.CustomerId.Value, out var importer))
                    {
                        request.ImporterName = importer.Name;
                    }
                }
            }
        }

        // VendorName — Vendors proxy.
        var vendorIds = requests.Where(r => r.VendorId.HasValue).Select(r => r.VendorId!.Value).Distinct().ToList();
        if (vendorIds.Count > 0)
        {
            var vendors = await vendorProxy.GetVendorsByIds(vendorIds);
            if (vendors != null)
            {
                var vendorsById = vendors.ToDictionary(v => v.Id);
                foreach (var request in requests)
                {
                    if (request.VendorId.HasValue && vendorsById.TryGetValue(request.VendorId.Value, out var vendor))
                    {
                        request.VendorName = vendor.Name;
                    }
                }
            }
        }

        // IssuingCountry name — shared Country lookup (raw id in IssuingCountryIdNum).
        await lookupUtil.FillName<Country, GetImportAuthenticationRequestResultDto>(
            requests,
            r => r.IssuingCountryIdNum ?? 0,
            (r, name) => r.IssuingCountryId = name);

        // OrganizationUnit name — shared OrganizationUnit lookup (raw id in OrganizationUnitIdNum).
        await lookupUtil.FillName<OrganizationUnit, GetImportAuthenticationRequestResultDto>(
            requests,
            r => r.OrganizationUnitIdNum ?? 0,
            (r, name) => r.OrganizationUnitId = name);

        // TODO(migration): LeadDocumentTitle stays null — it's a CRP.DealFile document (no lookup type; needs the
        // owning service's proxy, not yet established). The raw LeadDocumentId is returned for a later pass.
    }

    public async Task<List<GetAuthenticationRequestByLeadDocumentResultDto>> GetAuthenticationRequestByLeadDocumentIDs(List<int> leadDocumentIds)
    {
        var parameters = BuildLeadDocumentIdsParameter(leadDocumentIds);
        var result = await DataLayer.GetAuthenticationRequestByLeadDocumentIDs(parameters);
        await FillLeadDocumentRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildLeadDocumentIdsParameter(List<int> leadDocumentIds)
    {
        // Pass the id list as the Shared.IntArray table-valued parameter (@LeadDocumentIDs).
        var table = new DataTable();
        table.Columns.Add("val", typeof(int));
        if (leadDocumentIds != null)
        {
            foreach (var id in leadDocumentIds)
            {
                table.Rows.Add(id);
            }
        }

        var parameters = new DynamicParameters();
        parameters.Add("@LeadDocumentIDs", table.AsTableValuedParameter("Shared.IntArray"));
        return parameters;
    }

    private async Task FillLeadDocumentRequestNames(List<GetAuthenticationRequestByLeadDocumentResultDto> results)
    {
        if (results.Count == 0)
        {
            return;
        }

        // ImportCountryName + OrganizationUnitName via the shared lookups (raw ids returned by the SP).
        await lookupUtil.FillName<Country, GetAuthenticationRequestByLeadDocumentResultDto>(
            results,
            r => r.ImportCountryId ?? 0,
            (r, name) => r.ImportCountryName = name);

        await lookupUtil.FillName<OrganizationUnit, GetAuthenticationRequestByLeadDocumentResultDto>(
            results,
            r => r.OrganizationUnitId ?? 0,
            (r, name) => r.OrganizationUnitName = name);

        // TODO(migration): LeadDocumentTitle stays null — CRP.DealFile document, needs the owning service's proxy.
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var result = await DataLayer.CheckImporterOfImportAuthentication(importerId);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForVendor(vendorId);
        return result;
    }

    // The WCF took the full ImportAuthenticationRequest entity but used only these 4 scalar fields
    // (ImporterID, VendorId, CustomerID, IssuingCountryID). The @DaysForLastDelivery window stays inside the SP
    // (read from the local Infrastructure.Parameters), so it is not a BL/config concern here.
    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId)
    {
        var result = await DataLayer.CheckIfExistsAdditionalRequestsForImporter(importerId, vendorId, customerId, countryId);
        return result;
    }

    // Internal WCF: SaveImportAuthenticationRequest(request) — saves an import authentication request's central-decision
    // edits. Faithful to the WCF (developer decisions 2026-08-05): (1) the first collateral supplies CollateralId and
    // all collaterals are pushed to permanent (Collateral service) — collaterals are NOT a local child table; (2) the
    // decision switch raises the matching events (Tasks-service task checks reuse IsTaskExist) and, on a central
    // decision, sends the decision message (Message-Management service); (3) VendorId 0 → null; (4) AuthenticationNeedless
    // additionally raises a rejection event assigning the opened task to the responder. The persist is a set-based
    // update on the existing row (the entity has no child collection — ItemDetailID is a scalar). Missing row → 404.
    // Returns the fully re-read request graph via GetAuthenticationRequestByID — consistent with the sibling
    // SaveAuthenticationRequestFile (both saves return the same shape as their GetById read).
    public async Task<GetAuthenticationRequestByIdResultDto> SaveImportAuthenticationRequest(SaveImportAuthenticationRequestRequestDto request)
    {
        var tasksProxy = Resolve<ITasksProxy>();

        // The first collateral supplies CollateralId; all collaterals are converted from temporary to permanent.
        if (request.Collaterals.Count > 0)
        {
            request.CollateralId = request.Collaterals[0].CollateralRequestId;
            await ChangeTempCollateralRequest(request.Collaterals);
        }

        var eventUtil = Resolve<IEventUtil>();

        // Decision-driven events / message (mirrors the legacy switch).
        switch (request.DecisionId)
        {
            case (int)EAuthenticationRequestDecision.NewAuthenticationRequest:
            {
                // Open the SetDecisionBeforeAssociation task, unless the current user already handles the request or
                // such a task already exists (legacy IsTaskExistsOnEntity → reuse IsTaskExist).
                var setDecisionTasks = await tasksProxy.IsTaskExist(
                    request.DocumentId, (int)EEntityType.ImportAuthenticationRequest, [(int)ETaskType.SetDecisionBeforeAssociation]);
                if (!request.IsCurrentUserHandleRequest && (setDecisionTasks is null || setDecisionTasks.Count == 0))
                {
                    await RaiseNewRequestEvent(eventUtil, request);
                }

                break;
            }

            case (int)EAuthenticationRequestDecision.AuthenticationRequried:
            {
                // If a HandleRejectedAuthenticationRequest task exists, mark the request processed-after-rejection and
                // re-open the new-request flow.
                var rejectedTasks = await tasksProxy.IsTaskExist(
                    request.DocumentId, (int)EEntityType.ImportAuthenticationRequest, [(int)ETaskType.HandleRejectedAuthenticationRequest]);
                if (rejectedTasks is { Count: > 0 })
                {
                    var processedEvent = eventUtil.CreatBuilder()
                        .WithEventType((int)EEventType.ImportAuthenticationRequestProcessedWithWasRejected)
                        .WithEntityId(request.DocumentId)
                        .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                        .WithTitle(request.DocumentId.ToString())
                        .Build();
                    await eventUtil.RaiseEvent(processedEvent);
                    await RaiseNewRequestEvent(eventUtil, request);
                }

                break;
            }

            default:
            {
                // Close the SetDecisionBeforeAssociation task + notify the handling user(s) of the central decision.
                var decisionEvent = eventUtil.CreatBuilder()
                    .WithEventType((int)EEventType.NewDecisionBeforeAssociation)
                    .WithEntityId(request.DocumentId)
                    .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                    .WithTitle(request.DocumentId.ToString())
                    .WithAdditionalInfo(request.DocumentId.ToString())
                    .Build();
                await eventUtil.RaiseEvent(decisionEvent);
                await SendDecisionMessage(request.DocumentId, request.DecisionId, request.AuthenticationFileId, request.UserId, request.UserResponseId);
                break;
            }
        }

        // VendorId 0 → null (legacy normalization).
        if (request.VendorId == 0)
        {
            request.VendorId = null;
        }

        // AuthenticationNeedless additionally raises a rejection event, assigning the opened task to the responder.
        if (request.DecisionId == (int)EAuthenticationRequestDecision.AuthenticationNeedless)
        {
            var rejectedEvent = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.AuthenticationRequestRejected)
                .WithEntityId(request.DocumentId)
                .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                .WithTitle(request.DocumentId.ToString())
                .WithTaskArguments(task => task.WithTaskAssignmentUser(request.UserResponseId))
                .Build();
            await eventUtil.RaiseEvent(rejectedEvent);
        }

        // Persist (set-based update) — 404 if the request row is gone.
        var userId = RequestMetadata.UserId ?? 0;
        var saved = await DataLayer.SaveImportAuthenticationRequest(request, userId);
        if (!saved)
        {
            throw new RestNotFoundException();
        }

        // Return the fully re-read request graph (consistent with the sibling SaveAuthenticationRequestFile — the SPA
        // gets the same authoritative, enriched shape as the GetById read).
        return await GetAuthenticationRequestByID(request.DocumentId);
    }

    // Legacy RaiseNewRequestEvent — NewAuthenticationRequest event on the request (opens the SetDecisionBeforeAssociation
    // task). The file, if any, is added as a related entity.
    private static async Task RaiseNewRequestEvent(IEventUtil eventUtil, SaveImportAuthenticationRequestRequestDto request)
    {
        var builder = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.NewAuthenticationRequest)
            .WithEntityId(request.DocumentId)
            .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
            .WithTitle(request.DocumentId.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .WithAdditionalInfo(request.DocumentId.ToString());
        if (request.AuthenticationFileId.HasValue)
        {
            builder = builder.AddRelatedEntity(request.AuthenticationFileId.Value, (int)EEntityType.AuthenticationRequestFile);
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy SendDecisionMessage — notifies the responder (and the creating user, if different) of the central
    // decision (Message-Management service). Rejection uses a different message type + parameters. Shared by
    // SaveImportAuthenticationRequest (#31) and SaveAuthenticationRequestFile (#32, per changed child request).
    private async Task SendDecisionMessage(int documentId, int? decisionId, int? authenticationFileId, int userId, int userResponseId)
    {
        var messageManagementProxy = Resolve<IMessageManagementProxy>();

        // On creation UserID == UserResponseID; a later change makes them differ, so message both.
        var userIds = new List<int> { userResponseId };
        if (userId != userResponseId)
        {
            userIds.Add(userId);
        }

        var destinations = userIds.Select(id => new MessageDestinationDto { UserId = id }).ToList();

        var message = new SendMessageDto
        {
            RelatedEntity = new VirtualEntityDto
            {
                Id = documentId,
                EntityType = (int)EEntityType.ImportAuthenticationRequest,
            },
            MessageTypeId = (int)EMessageTypes.ImportRequestCentralDecision,
        };

        switch (decisionId)
        {
            case (int)EAuthenticationRequestDecision.AuthenticationRequried:
            case (int)EAuthenticationRequestDecision.AuthenticationNeedless:
            case (int)EAuthenticationRequestDecision.Approval:
            case (int)EAuthenticationRequestDecision.Partly:
            case (int)EAuthenticationRequestDecision.DemandAnotherClarification:
            {
                var decisionName = await GetDecisionName(decisionId ?? 0);
                message.MessageParameters = [decisionName, documentId.ToString()];
                break;
            }

            case (int)EAuthenticationRequestDecision.Rejection:
            {
                message.MessageTypeId = (int)EMessageTypes.ImportRequestRejection;
                message.MessageParameters = [documentId.ToString(), authenticationFileId?.ToString() ?? string.Empty];
                break;
            }
        }

        if (destinations.Count > 1)
        {
            message.IsGroupMessage = true;
            message.MultipleMessageDestinations = destinations;
        }
        else
        {
            message.UserIdToSendMessage = userResponseId;
        }

        await messageManagementProxy.SendMessage(message);
    }

    // Legacy SystemTablesUtil.GetCodeById<CertificateOfOriginsDecision>(decisionId).Name — the decision's Hebrew name
    // from the enum_Decision lookup table (used as a message parameter).
    private async Task<string> GetDecisionName(int decisionId)
    {
        var decisions = await DataLayer.GetAllDecisions();
        return decisions.FirstOrDefault(d => d.Id == decisionId)?.Name ?? string.Empty;
    }

    // Legacy ChangeTempCollateralRequest — converts the request's temporary collaterals into permanent ones bound to
    // the request (Collateral service).
    private async Task ChangeTempCollateralRequest(List<CollateralRequestDto> collaterals)
    {
        var collateralProxy = Resolve<ICollateralProxy>();

        var payload = collaterals
            .Select(collateral => new ChangeTempCollateralRequestDto
            {
                CollateralRequestId = collateral.CollateralRequestId,
                RelatedEntityId = collateral.RelatedEntity?.Id ?? 0,
                EntityExternalId = (collateral.RelatedEntity?.Id ?? 0).ToString(),
            })
            .ToList();
        await collateralProxy.ChangeTempCollateralRequest(payload);
    }

    // Internal WCF: SaveAuthenticationRequestFile(file) — saves an authentication file's central-decision review. Per
    // changed child request: raises close-tasks + decision-update events, sends the decision message, and (on Approval
    // with collaterals) grants them. Per file-status change: opens/closes the file tasks, handles the cancel/reminder/
    // final transitions, raises the file status-update event, and sends the file status message. Faithful to the WCF
    // (developer decisions 2026-08-05): updates are set-based (trust-client); the file + requests pre-exist (UPDATE
    // only); the legacy WPF-only AuthenticationFileStatusIDPrev guard is replaced by the load snapshot
    // OriginalAuthenticationFileStatusId (kept fresh by the save re-read). Returns the fully re-read file.
    public async Task<GetAuthenticationRequestFileByIdResultDto> SaveAuthenticationRequestFile(SaveAuthenticationRequestFileRequestDto request)
    {
        var userId = RequestMetadata.UserId ?? 0;

        // 1. Persist every child request's decision + recomputed IsOldIndication (3+ years since issuing).
        var threeYearsAgo = DateTimeOffset.Now.AddYears(-3);
        foreach (var child in request.Requests)
        {
            var isOldIndication = child.DocumentIssuingDate <= threeYearsAgo;
            await DataLayer.UpdateImportRequestDecision(child.DocumentId, child.DecisionId, isOldIndication, userId);
        }

        // 2. Per changed child: events + decision message (+ grant collaterals on approval).
        await ManageRequestStatus(request);

        // 3. File-status change: tasks + status events/message.
        await ManageFileStatus(request);

        // 4. Persist the file's own scalar edits — 404 if the file row is gone.
        var saved = await DataLayer.UpdateAuthenticationFile(request, userId);
        if (!saved)
        {
            throw new RestNotFoundException();
        }

        // 5. Return the fully re-read file (the legacy returns GetAuthenticationRequestFileByID(id)).
        return await GetAuthenticationRequestFileByID(request.Id);
    }

    // Legacy ManageImportAuthenticationRequestStatus — for each child whose decision changed vs the load snapshot:
    // close its open tasks (unless "another clarification"), send the decision message, grant collaterals on approval,
    // and log the decision change.
    private async Task ManageRequestStatus(SaveAuthenticationRequestFileRequestDto request)
    {
        var collateralProxy = Resolve<ICollateralProxy>();
        var eventUtil = Resolve<IEventUtil>();
        foreach (var child in request.Requests)
        {
            if (child.DecisionId == child.OriginalRequestDecisionId)
            {
                continue;
            }

            var collateralIds = await collateralProxy.GetCollateralRequestIdsByRelatedEntity(
                (int)EEntityType.ImportAuthenticationRequest, child.DocumentId) ?? [];

            if (child.DecisionId != (int)EAuthenticationRequestDecision.DemandAnotherClarification)
            {
                var closeTasks = eventUtil.CreatBuilder()
                    .WithEventType((int)EEventType.CloseAllTaskForImportAuthenticationRequest)
                    .WithEntityId(child.DocumentId)
                    .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                    .WithTitle(child.DocumentId.ToString())
                    .WithOrganizationUnitId(child.OrganizationUnitId);
                if (child.AuthenticationFileId.HasValue)
                {
                    closeTasks = closeTasks.AddRelatedEntity(child.AuthenticationFileId.Value, (int)EEntityType.AuthenticationRequestFile);
                }

                await eventUtil.RaiseEvent(closeTasks.Build());
            }

            await SendDecisionMessage(child.DocumentId, child.DecisionId, child.AuthenticationFileId, child.UserId, child.UserResponseId);

            // Legacy quirk preserved: the grant's EntityID is the FILE id (with EntityType ImportAuthenticationRequest).
            if (child.DecisionId == (int)EAuthenticationRequestDecision.Approval && collateralIds.Count > 0)
            {
                await collateralProxy.GrantAllCollateralRequests(
                [
                    new GrantCollateralRequestDto
                    {
                        EntityId = request.Id,
                        EntityTypeId = (int)EEntityType.ImportAuthenticationRequest,
                    },
                ]);
            }

            await RaiseRequestDecisionUpdateEvent(eventUtil, child);
        }
    }

    // Legacy RaiseEventForAuthenticationRequest — logs the decision change on the request (AdditionalInfo = decision
    // name + acting user + date). The file, if any, is a related entity.
    private async Task RaiseRequestDecisionUpdateEvent(IEventUtil eventUtil, SaveAuthenticationRequestFileChildDto child)
    {
        var builder = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.AuthenticationRequestDecisionUpdate)
            .WithEntityId(child.DocumentId)
            .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
            .WithTitle(child.DocumentId.ToString());
        if (child.AuthenticationFileId.HasValue)
        {
            builder = builder.AddRelatedEntity(child.AuthenticationFileId.Value, (int)EEntityType.AuthenticationRequestFile);
        }

        if (child.DecisionId.HasValue)
        {
            var decisionName = await GetDecisionName(child.DecisionId.Value);
            builder = builder.WithAdditionalInfo(FormatStatusUpdateInfo(decisionName));
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy ManageImportAuthenticationFileStatus — only when the file status changed vs the load snapshot: open/close
    // the file tasks, handle the cancel/reminder/final transitions, then raise the file status-update event + message.
    private async Task ManageFileStatus(SaveAuthenticationRequestFileRequestDto request)
    {
        if (request.AuthenticationFileStatusId == request.OriginalAuthenticationFileStatusId)
        {
            return;
        }

        var eventUtil = Resolve<IEventUtil>();
        var userId = RequestMetadata.UserId ?? 0;

        await CheckStatusAndOpenTask(eventUtil, request, userId);

        if (request.AuthenticationFileStatusId != (int)EAuthenticationFileStatus.ClarificationRequired)
        {
            var closeFileTasks = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.CloseAllTaskForImportAuthenticationRequestFile)
                .WithEntityId(request.Id)
                .WithEntityType((int)EEntityType.AuthenticationRequestFile)
                .WithTitle(request.Id.ToString())
                .WithOrganizationUnitId(request.OrganizationUnitId)
                .Build();
            await eventUtil.RaiseEvent(closeFileTasks);
        }

        foreach (var child in request.Requests.Where(c => c.DecisionId == (int)EAuthenticationRequestDecision.Rejection))
        {
            var rejected = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.AuthenticationRequestRejected)
                .WithEntityId(child.DocumentId)
                .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
                .WithTitle(child.DocumentId.ToString())
                .WithOrganizationUnitId(child.OrganizationUnitId)
                .AddRelatedEntity(request.Id, (int)EEntityType.AuthenticationRequestFile)
                .WithTaskArguments(t => t.WithOpenTaskBehaviour(OpenTaskBehaviour.CloseOld))
                .Build();
            await eventUtil.RaiseEvent(rejected);
        }

        await RaiseFileStatusUpdateEvent(eventUtil, request);
        await RaiseStatusMessage(request);
    }

    // Legacy CheckStatusAndOpenTask — status-specific side effects: open the handling task for the answer/clarification
    // statuses (or when any child is a partial decision); on cancellation detach the child requests; and fire the
    // vendor-reminder / final-decision events on those specific target statuses (the legacy WPF-only "Prev" guard is
    // dropped — the enclosing status-changed check already ensures a real transition).
    private async Task CheckStatusAndOpenTask(IEventUtil eventUtil, SaveAuthenticationRequestFileRequestDto request, int userId)
    {
        var openTaskStatuses = new[]
        {
            (int)EAuthenticationFileStatus.ReceivedAnswerInFile,
            (int)EAuthenticationFileStatus.RightAuthenticationAnswer,
            (int)EAuthenticationFileStatus.ClarificationRequired,
            (int)EAuthenticationFileStatus.WrongAuthenticationAnswer,
        };
        if (openTaskStatuses.Contains(request.AuthenticationFileStatusId)
            || request.Requests.Any(c => c.DecisionId == (int)EAuthenticationRequestDecision.Partly))
        {
            var handle = eventUtil.CreatBuilder()
                .WithEventType((int)EEventType.HandleImportAuthenticationRequest)
                .WithEntityId(request.Id)
                .WithEntityType((int)EEntityType.AuthenticationRequestFile)
                .WithTitle(request.Id.ToString())
                .WithOrganizationUnitId(request.OrganizationUnitId)
                .WithTaskArguments(t => t.WithOpenTaskBehaviour(OpenTaskBehaviour.CloseOld))
                .Build();
            await eventUtil.RaiseEvent(handle);
        }

        if (request.AuthenticationFileStatusId == (int)EAuthenticationFileStatus.CancelledFile)
        {
            await DataLayer.UnlinkAllRequestsFromFile(request.Id, userId);
        }

        if (request.AuthenticationFileStatusId == (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend)
        {
            await RaiseFileEvent(eventUtil, request, (int)EEventType.UpdateFileStatusVendorReminderNotice);
        }

        if (request.AuthenticationFileStatusId == (int)EAuthenticationFileStatus.ReceivedAnswerInFile)
        {
            await RaiseFileEvent(eventUtil, request, (int)EEventType.UpdateFileStatusFinalDecisionInCase);
        }
    }

    // A bare file-scoped event (VirtualEntity(file)) — used for the vendor-reminder / final-decision transitions.
    private static async Task RaiseFileEvent(IEventUtil eventUtil, SaveAuthenticationRequestFileRequestDto request, int eventTypeId)
    {
        var evt = eventUtil.CreatBuilder()
            .WithEventType(eventTypeId)
            .WithEntityId(request.Id)
            .WithEntityType((int)EEntityType.AuthenticationRequestFile)
            .WithTitle(request.Id.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .Build();
        await eventUtil.RaiseEvent(evt);
    }

    // Legacy RaiseEventForFile — logs the file status change (AdditionalInfo = file-status name + acting user + date).
    private async Task RaiseFileStatusUpdateEvent(IEventUtil eventUtil, SaveAuthenticationRequestFileRequestDto request)
    {
        var statusName = await GetFileStatusName(request.AuthenticationFileStatusId);
        var evt = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.AuthenticationRequestFileStatusUpdate)
            .WithEntityId(request.Id)
            .WithEntityType((int)EEntityType.AuthenticationRequestFile)
            .WithTitle(request.Id.ToString())
            .WithAdditionalInfo(FormatStatusUpdateInfo(statusName))
            .Build();
        await eventUtil.RaiseEvent(evt);
    }

    // Legacy RaiseStatusMessage — notifies the file's issuing user of the new file status (Message-Management service).
    private async Task RaiseStatusMessage(SaveAuthenticationRequestFileRequestDto request)
    {
        var messageManagementProxy = Resolve<IMessageManagementProxy>();

        var statusName = await GetFileStatusName(request.AuthenticationFileStatusId);
        var message = new SendMessageDto
        {
            RelatedEntity = new VirtualEntityDto
            {
                Id = request.Id,
                EntityType = (int)EEntityType.AuthenticationRequestFile,
            },
            MessageTypeId = (int)EMessageTypes.ImportRequestDecision,
            MessageParameters = [request.Id.ToString(), statusName],
            MultipleMessageDestinations = [new MessageDestinationDto { UserId = request.UserId }],
        };
        await messageManagementProxy.SendMessage(message);
    }

    // Legacy SystemTablesUtil.GetCodeById<CertificateOfOriginsAuthenticationFileStatus>(id).Name — the file-status
    // Hebrew name from the enum_AuthenticationFileStatus lookup table.
    private async Task<string> GetFileStatusName(int statusId)
    {
        var statuses = await DataLayer.GetAllFileStatuses();
        return statuses.FirstOrDefault(s => s.Id == statusId)?.Name ?? string.Empty;
    }

    // Legacy AdditionalInfo string on the status/decision events: "עודכן הסטאטוס ל{name} על ידי {user} בתאריך {date} ".
    private string FormatStatusUpdateInfo(string name)
    {
        return string.Format(
            "עודכן הסטאטוס ל{0} על ידי {1} בתאריך {2} ",
            name,
            RequestMetadata.Fullname,
            DateTime.Today.ToShortDateString());
    }

    #region LEGACY_WCF
#pragma warning disable S125 // migration convention: preserve the original WCF body as a reference comment

    // Original WCF (AuthenticationRequestBL.CheckImporterOfImportAuthentication):
    //
    // public int? CheckImporterOfImportAuthentication(int importerId)
    // {
    //     return _uow.Repository.GetQuery<VerificationProhibitedImporters>()
    //         .FirstOrDefault(c => c.CustomerId == importerId)?.ID == null ? importerId : (int?)null;
    // }
    //
    // Returns the importer id when the importer is NOT on the prohibited list; null when it is.
#pragma warning restore S125
    #endregion

    // Internal WCF: GetPathsForNavigationToVendor() — the navigation-path tree for the fixed "navigate to vendor"
    // PathID (legacy AuthenticationRequestBL.GetPathsForNavigationToVendor). The legacy read NavigationPath rows
    // (shared Infrastructure/Common GeneralServices table) via the UoW repository, filtered by PathID, and mapped them
    // to the view tree; only PathId + ViewPaths were populated (ViewName/ViewId/IsMandatory left default — bug-for-bug).
#pragma warning disable CA1822, S125 // CA1822: instance method by design (uses ILookupUtil once wired); S125: the legacy-mapping reference in the TODO is intentional
    public Task<NavigationToVendorViewDto> GetPathsForNavigationToVendor()
    {
        var pathId = CertificateOfOriginsConsts.NavigationToVendorPathId;

        // TODO(blocking): NavigationPath is a shared GeneralServices reference table with no platform lookup type yet.
        // Per the product decision (2026-08) it will be exposed as a platform Lookup — resolve it here via ILookupUtil
        // once InfrastructureCore.Lookup adds a `NavigationPath` lookup type AND a source service exposes
        // GET /lookup/NavigationPath (both done internally — see INTERNAL_INTEGRATION.md). Until then ViewPaths is empty.
        // When wired, this method becomes `async` and awaits the lookup:
        //   var paths = (await lookupUtil.Search<NavigationPath>(p => p.PathId == pathId)).ToList();
        //   viewPaths = paths.Select(p => new NavigationToVendorPathDto
        //   {
        //       Id = p.PathRouteId,
        //       PathId = p.PathId,
        //       PageNameId = p.PageNameId,
        //       ParentPathRouteId = p.ParentPathRouteId,
        //       ViewId = p.ViewId,
        //       Name = p.PageNameId.HasValue ? p.PageName : p.ViewName,
        //   }).ToList();
        var viewPaths = new List<NavigationToVendorPathDto>();

        return Task.FromResult(new NavigationToVendorViewDto
        {
            PathId = pathId,
            ViewPaths = viewPaths,
        });
    }
#pragma warning restore CA1822, S125
}
