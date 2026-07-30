using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Dapper;
using Lookup;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class AuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    IVendorProxy vendorProxy,
    IDocumentsProxy documentsProxy,
    IParametersUtil parametersUtil,
    ILookupUtil lookupUtil)
    : BaseBL<AuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
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
        else if (status == (int)EAuthenticationFileStatus.AuthenticationRequestReminderWasSend)
        {
            if (deliveryMethod == (int)EDeliveryMethod.FirstRemindSent)
            {
                deliveryMethod = (int)EDeliveryMethod.SecondRemindSent;
            }
        }

        return (status, deliveryMethod);
    }

    // Internal WCF: GetEntityDocuments(importAuthenticationRequest) — the WCF took the full request entity but used
    // only its LeadDocumentID, so it is flattened to that scalar here (same precedent as
    // CheckIfExistsAdditionalRequestsForImporter). Returns the entity's documents (from the Documents service),
    // filtered to the allowed document types and to documents not already requested / claimed by another lead doc.
    public async Task<List<DocumentDto>> GetEntityDocuments(int leadDocumentId)
    {
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
}
