using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Dapper;
using Lookup;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public class ExportDocumentAuthenticationRequestBl(
    IServiceProvider serviceProvider,
    ICustomerProxy customerProxy,
    IDocumentsProxy documentsProxy,
    IMessageManagementProxy messageManagementProxy,
    ILookupUtil lookupUtil)
    : BaseBL<ExportDocumentAuthenticationRequestBl, ICertificateOfOriginsDal>(serviceProvider)
{
    // EMessageTypes.ImportRequestDecision (Customs.Inf.CommonService.ExternalCommon.MessagesEnums) — the message
    // type sent on an export-request status change.
    private const int ImportRequestDecisionMessageTypeId = 11102;

    public async Task<CustomerDto> GetCustomerInformation(int customerId)
    {
        // Single-customer lookup against the Customers service by id; the legacy threw on a missing customer,
        // so a not-found id owns the 404 contract. Address selection was client-side (SPA), not in the BL.
        var customer = await customerProxy.GetCustomerInformation(customerId)
            ?? throw new RestNotFoundException();
        return customer;
    }

    public async Task<CustomerDto> GetCustomerInformationByCountry(int countryId)
    {
        // Foreign customs-houses in the given country (Customers service, activity-type filtered in the proxy).
        // The legacy threw when the country had none, so an empty result owns the 404 contract, and it returned
        // the first candidate (FirstOrDefault over the activity-type-filtered list).
        var customers = await customerProxy.GetCustomersByCountry(countryId);
        if (customers is null || customers.Count == 0)
        {
            throw new RestNotFoundException();
        }

        return customers[0];
    }

    public async Task<GetExportDocumentAuthenticationRequestByIdResultDto> GetExportDocumentAuthenticationRequestById(int id)
    {
        // Single request by id + its three child collections; a missing id owns the 404 contract (the legacy
        // .Single threw, so no-match is not a valid result).
        var entity = await DataLayer.GetExportDocumentAuthenticationRequestById(id)
            ?? throw new RestNotFoundException();

        // Map entity + children. OriginalStatusId snapshots the status for the later optimistic dirty-check on
        // Save. ExportDeclarationIds replaces the legacy EntityTypeAndIDsToSearch dictionary (which only drove the
        // old WPF document-attach picker): the lead-document ids the client can attach documents to.
        var result = new GetExportDocumentAuthenticationRequestByIdResultDto
        {
            Id = entity.Id,
            TypeId = entity.TypeId,
            Title = entity.Title,
            TimeStamp = entity.TimeStamp,
            CustomerId = entity.CustomerId,
            AuthenticationDocumentTypeId = entity.AuthenticationDocumentTypeId,
            ExporterCustomerId = entity.ExporterCustomerId,
            StatusId = entity.StatusId,
            OriginalStatusId = entity.StatusId ?? 0,
            CountryId = entity.CountryId,
            CustomsHouseAddress = entity.CustomsHouseAddress,
            VendorId = entity.VendorId,
            AuthenticationRequestArrivalDate = entity.AuthenticationRequestArrivalDate,
            AuthenticationRequestedByName = entity.AuthenticationRequestedByName,
            AuthenticationRequestedByEmail = entity.AuthenticationRequestedByEmail,
            AuthenticationRequestedByPhone = entity.AuthenticationRequestedByPhone,
            AuthenticationRequestNotes = entity.AuthenticationRequestNotes,
            ExportLeadDocumentId = entity.ExportLeadDocumentId,
            DocumentId = entity.DocumentId,
            MainDocumentTitle = entity.MainDocumentTitle,
            LastDeliveryDate = entity.LastDeliveryDate,
            DeliveryMethodId = entity.DeliveryMethodId,
            InvoiceNumbers = entity.InvoiceNumbers,
            DetailedDecision = entity.DetailedDecision,
            ReferenceNumber = entity.ReferenceNumber,
            CommentForCustomsHouseLetter = entity.CommentForCustomsHouseLetter,
            TotalDocuments = entity.TotalDocuments,
            TotalInvoices = entity.TotalInvoices,
            DocumentDate = entity.DocumentDate,
            InvoiceDate = entity.InvoiceDate,
            CustomsItems = entity.CustomsItems.Select(i => new ExportDocumentAuthenticationRequestCustomsItemDto
            {
                Id = i.Id,
                ExportDocumentAuthenticationRequestId = i.ExportDocumentAuthenticationRequestId,
                CustomsItemId = i.CustomsItemId,
            }).ToList(),
            LeadDocuments = entity.LeadDocuments.Select(l => new ExportDocumentAuthenticationRequestLeadDocumentDto
            {
                Id = l.Id,
                ExportRequestId = l.ExportRequestId,
                LeadDocumentId = l.LeadDocumentId,
                LeadDocumentTitle = l.LeadDocumentTitle,
            }).ToList(),
            ManufacturingAreas = entity.ManufacturingAreas.Select(m => new ExportAuthenticationRequestManufacturingAreaDto
            {
                Id = m.Id,
                ExportAuthenticationRequestId = m.ExportAuthenticationRequestId,
                ManufacturingArea = m.ManufacturingArea,
                ManufacturingZipcode = m.ManufacturingZipcode,
            }).ToList(),
            ExportDeclarationIds = entity.LeadDocuments
                .Where(l => l.LeadDocumentId.HasValue)
                .Select(l => l.LeadDocumentId!.Value)
                .ToList(),
        };
        return result;
    }

    // Internal WCF: SaveExportDocumentAuthenticationRequest(entity) — inserts (Id == 0) or updates the export-document
    // authentication request + its three child collections (replace-all, developer decision 2026-08-05); on a status
    // transition raises the status events and (for some statuses) sends the status message; finally attaches any
    // additional documents. Returns the freshly-saved graph. current-user = RequestMetadata.UserId, DisplayName =
    // RequestMetadata.Fullname. The legacy status name came from a SystemTable lookup — resolved here from the
    // EExportAuthenticationRequestStatus Display name (no ILookupUtil type exists, consistent with #26).
    public async Task<GetExportDocumentAuthenticationRequestByIdResultDto> SaveExportDocumentAuthenticationRequest(SaveExportDocumentAuthenticationRequestRequestDto request)
    {
        var entity = BuildEntity(request);

        var userId = RequestMetadata.UserId ?? 0;
        var now = DateTime.Now;
        if (entity.Id == 0)
        {
            entity.CreateDate = now;
            entity.CreateUserId = userId;
        }

        entity.UpdateDate = now;
        entity.UpdateUserId = userId;

        var id = await DataLayer.SaveExportDocumentAuthenticationRequest(entity);

        // Status transition → status-update event (+ a status-specific event) and, for some statuses, a message.
        if (request.StatusId != request.OriginalStatusId)
        {
            await CheckStatusAndNotify(id, entity.StatusId);
        }

        // Post-save document attach (legacy IDocumentServiceAdapter.AttachDocumentsToEntity).
        if (request.ListOfAdditionalDocumentsIds.Count > 0)
        {
            await documentsProxy.AttachDocumentsToEntity(new DocumentsToEntityDto
            {
                Entity = new VirtualEntityDto { Id = id, EntityType = (int)EEntityType.ExportDocumentAuthenticationRequest },
                DocumentIds = request.ListOfAdditionalDocumentsIds,
            });
        }

        return await GetExportDocumentAuthenticationRequestById(id);
    }

    private static ExportDocumentAuthenticationRequest BuildEntity(SaveExportDocumentAuthenticationRequestRequestDto request)
    {
        return new ExportDocumentAuthenticationRequest
        {
            Id = request.Id,
            TypeId = request.TypeId,
            Title = request.Title,
            State = request.State,
            TimeStamp = request.TimeStamp!,
            OrganizationUnitId = request.OrganizationUnitId,
            CustomerId = request.CustomerId,
            AuthenticationDocumentTypeId = request.AuthenticationDocumentTypeId,
            ExporterCustomerId = request.ExporterCustomerId,
            StatusId = request.StatusId,
            CountryId = request.CountryId,
            CustomsHouseAddress = request.CustomsHouseAddress,
            VendorId = request.VendorId,
            AuthenticationRequestArrivalDate = request.AuthenticationRequestArrivalDate,
            AuthenticationRequestedByName = request.AuthenticationRequestedByName,
            AuthenticationRequestedByEmail = request.AuthenticationRequestedByEmail,
            AuthenticationRequestedByPhone = request.AuthenticationRequestedByPhone,
            AuthenticationRequestNotes = request.AuthenticationRequestNotes,
            ExportLeadDocumentId = request.ExportLeadDocumentId,
            DocumentId = request.DocumentId,
            MainDocumentTitle = request.MainDocumentTitle,
            LastDeliveryDate = request.LastDeliveryDate,
            DeliveryMethodId = request.DeliveryMethodId,
            InvoiceNumbers = request.InvoiceNumbers,
            DetailedDecision = request.DetailedDecision,
            ReferenceNumber = request.ReferenceNumber,
            CommentForCustomsHouseLetter = request.CommentForCustomsHouseLetter,
            TotalDocuments = request.TotalDocuments,
            TotalInvoices = request.TotalInvoices,
            DocumentDate = request.DocumentDate,
            InvoiceDate = request.InvoiceDate,
            CustomsItems = request.CustomsItems.Select(item => new CustomsItemToExportDocumentAuthenticationRequest
            {
                Id = item.Id,
                ExportDocumentAuthenticationRequestId = item.ExportDocumentAuthenticationRequestId,
                CustomsItemId = item.CustomsItemId,
            }).ToList(),
            LeadDocuments = request.LeadDocuments.Select(leadDocument => new ExportDocumentAuthenticationRequestLeadDocument
            {
                Id = leadDocument.Id,
                ExportRequestId = leadDocument.ExportRequestId,
                LeadDocumentId = leadDocument.LeadDocumentId,
                LeadDocumentTitle = leadDocument.LeadDocumentTitle,
            }).ToList(),
            ManufacturingAreas = request.ManufacturingAreas.Select(area => new ExportAuthenticationRequestManufacturingArea
            {
                Id = area.Id,
                ExportAuthenticationRequestId = area.ExportAuthenticationRequestId,
                ManufacturingArea = area.ManufacturingArea,
                ManufacturingZipcode = area.ManufacturingZipcode,
            }).ToList(),
        };
    }

    // Legacy CheckStatus: a switch on the new status decides which status-specific event to raise (in addition to the
    // always-raised status-update event), and whether to also send a status message to the current user.
    private async Task CheckStatusAndNotify(int id, int? statusId)
    {
        switch (statusId)
        {
            case (int)EExportAuthenticationRequestStatus.ReadyForProfessionalTreatment:
                await RaiseStatusEvents(id, statusId, EEventType.ExportNewAuthenticationRequest, id.ToString());
                await SendStatusMessage(id, statusId);
                break;
            case (int)EExportAuthenticationRequestStatus.ClosedValid:
            case (int)EExportAuthenticationRequestStatus.ClosedNotValid:
            case (int)EExportAuthenticationRequestStatus.ClosedSemiValid:
                await RaiseStatusEvents(id, statusId, EEventType.ExportAuthenticationRequestAfterClosing, null);
                break;
            case (int)EExportAuthenticationRequestStatus.Cancelled:
            case (int)EExportAuthenticationRequestStatus.WaitingForExporter:
                await RaiseStatusEvents(id, statusId, EEventType.ChangeFileStatus, null);
                break;
            default:
                await RaiseStatusEvents(id, statusId, null, null);
                await SendStatusMessage(id, statusId);
                break;
        }
    }

    // Always raises ExportAuthenticationRequestFileStatusUpdate (with the "changed by X on date" info), then a
    // status-specific event when one applies.
    private async Task RaiseStatusEvents(int id, int? statusId, EEventType? specificEvent, string? additionalInfo)
    {
        var eventUtil = Resolve<IEventUtil>();
        var updateInfo = string.Format(
            "עודכן הסטאטוס ל{0} על ידי {1} בתאריך {2} ",
            GetStatusName(statusId),
            RequestMetadata.Fullname,
            DateTime.Today.ToShortDateString());

        var statusUpdate = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.ExportAuthenticationRequestFileStatusUpdate)
            .WithEntityId(id)
            .WithEntityType((int)EEntityType.ExportDocumentAuthenticationRequest)
            .WithTitle(id.ToString())
            .WithAdditionalInfo(updateInfo)
            .Build();
        await eventUtil.RaiseEvent(statusUpdate);

        if (specificEvent.HasValue)
        {
            var specific = eventUtil.CreatBuilder()
                .WithEventType((int)specificEvent.Value)
                .WithEntityId(id)
                .WithEntityType((int)EEntityType.ExportDocumentAuthenticationRequest)
                .WithTitle(id.ToString())
                .WithAdditionalInfo(additionalInfo ?? string.Empty)
                .Build();
            await eventUtil.RaiseEvent(specific);
        }
    }

    // Legacy RaiseStatusMessage: send the current user a message (file id + new status name) via Message-Management.
    private async Task SendStatusMessage(int id, int? statusId)
    {
        var message = new SendMessageDto
        {
            RelatedEntity = new VirtualEntityDto { Id = id, EntityType = (int)EEntityType.ExportDocumentAuthenticationRequest },
            MessageTypeId = ImportRequestDecisionMessageTypeId,
            MessageParameters = [id.ToString(), GetStatusName(statusId)],
            MultipleMessageDestinations = [new MessageDestinationDto { UserId = RequestMetadata.UserId }],
        };
        await messageManagementProxy.SendMessage(message);
    }

    // The status display name (legacy SystemTablesUtil.GetCodeById<ExportAuthenticationRequestStatus>.Name) — taken
    // from the EExportAuthenticationRequestStatus [Display(Name)] attribute.
    private static string GetStatusName(int? statusId)
    {
        if (statusId is null)
        {
            return string.Empty;
        }

        var status = (EExportAuthenticationRequestStatus)statusId.Value;
        var member = typeof(EExportAuthenticationRequestStatus).GetMember(status.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? status.ToString();
    }

    public async Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetExportDocumentAuthenticationRequestSearch(parameters);
        await FillExportRequestNames(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CountryID", filter.CountryId, DbType.Int32);
        parameters.Add("@DocumentTypeID", filter.DocumentTypeId, DbType.Int32);
        parameters.Add("@RequestID", filter.RequestId, DbType.Int32);
        parameters.Add("@ForeignCustomsHouseID", filter.ForeignCustomsHouseId, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@RequestOpenDateFrom", filter.RequestOpenDateFrom, DbType.DateTime);
        parameters.Add("@RequestOpenDateTo", filter.RequestOpenDateTo, DbType.DateTime);
        parameters.Add("@ExportAuthenticationDocumentID", filter.ExportAuthenticationDocumentId, DbType.Int32);
        parameters.Add("@InvoiceIDNum", filter.InvoiceIdNum, DbType.String);
        parameters.Add("@MainDocumentTitle", filter.MainDocumentTitle, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@ExportAuthenticationRequestStatusID", filter.ExportAuthenticationRequestStatusId, DbType.Int32);
        parameters.Add("@CreateUserID", filter.CreateUserId, DbType.Int32);
        return parameters;
    }

    private async Task FillExportRequestNames(List<GetExportDocumentAuthenticationRequestSearchResultDto> results)
    {
        if (results.Count == 0)
        {
            return;
        }

        // ForeignCustomsHouseName (from CustomerId) + RequestIssuerName (from ExporterCustomerId) — both Customers proxy.
        var customerIds = results.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId!.Value)
            .Concat(results.Where(r => r.ExporterCustomerId.HasValue).Select(r => r.ExporterCustomerId!.Value))
            .Distinct()
            .ToList();
        if (customerIds.Count > 0)
        {
            var customers = await customerProxy.GetCustomersByIds(customerIds);
            if (customers != null)
            {
                var customersById = customers.ToDictionary(c => c.Id);
                foreach (var result in results)
                {
                    if (result.CustomerId.HasValue && customersById.TryGetValue(result.CustomerId.Value, out var foreignCustomsHouse))
                    {
                        result.ForeignCustomsHouseName = foreignCustomsHouse.Name;
                    }

                    if (result.ExporterCustomerId.HasValue && customersById.TryGetValue(result.ExporterCustomerId.Value, out var issuer))
                    {
                        result.RequestIssuerName = issuer.Name;
                    }
                }
            }
        }

        // CountryName via the shared Country lookup (raw id in CountryId).
        await lookupUtil.FillName<Country, GetExportDocumentAuthenticationRequestSearchResultDto>(
            results,
            r => r.CountryId ?? 0,
            (r, name) => r.CountryName = name);
    }
}
