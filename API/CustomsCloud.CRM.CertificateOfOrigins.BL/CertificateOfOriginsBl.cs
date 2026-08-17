using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Queue;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

public partial class CertificateOfOriginsBl(IServiceProvider serviceProvider, ICustomerProxy customerProxy, IExportDealFileProxy exportDealFileProxy, IUserProxy userProxy, IDataDictionaryFieldProxy dataDictionaryFieldProxy, ICurrencyTypeProxy currencyTypeProxy, IDocumentsProxy documentsProxy, ICustomsBookProxy customsBookProxy, ICommonServicesProxy commonServicesProxy, IOrganizationUnitProxy organizationUnitProxy, IMessageManagementProxy messageManagementProxy, ICountryGroupProxy countryGroupProxy, ITasksProxy tasksProxy, ILockUtil lockUtil, ILookupUtil lookupUtil, IParametersUtil parametersUtil, ICountryProxy countryProxy, ISiteProxy siteProxy, IInternationalSiteProxy internationalSiteProxy, IPackingTypeProxy packingTypeProxy, IMeasurementUnitProxy measurementUnitProxy)
    : BaseBL<CertificateOfOriginsBl, ICertificateOfOriginsDal>(serviceProvider)
{
    public async Task<CertificateOfOriginDto> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        // Single certificate with its full graph (7 result sets). Missing id → 404 (the legacy returned null,
        // which callers treated as not-found). Milestone user display-names are enriched here — the SP returns only
        // the acting user id (the cross-service Infrastructure.UserMng_User JOIN was removed).
        var certificate = await DataLayer.GetCertificateOfOriginById(certificateOfOriginId)
            ?? throw new RestNotFoundException();
        await FillMilestoneUserNames(certificate);
        return certificate;
    }

    private async Task FillMilestoneUserNames(CertificateOfOriginDto certificate)
    {
        var userIds = certificate.Milestones
            .Where(m => m.UserId.HasValue)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var users = await userProxy.GetUsersByIds(userIds);
        if (users == null)
        {
            return;
        }

        var usersById = users.ToDictionary(u => u.Id);
        foreach (var milestone in certificate.Milestones)
        {
            if (milestone.UserId.HasValue && usersById.TryGetValue(milestone.UserId.Value, out var user))
            {
                milestone.UserName = user.Name;
            }
        }
    }

    #region GetPC_MSG2280_2281 (Incoming / EAI certificate-of-origin request)

    // Incoming/EAI WCF: GetPC_MSG2280_2281_CertificateOfOriginRequest (PC_NG_2280 → feedback PC_NG_2281). Processes an
    // agent's certificate-of-origin request and returns the feedback synchronously — the legacy one-way callback/MSMQ
    // response is exposed here as a direct REST return (developer decision: mirrors the legacy *Sync contract and the
    // migrated sibling GetCertificateRequestByGuid).
    //
    // SKELETON (developer decision): only the read/cancel branches (GetRequestStatus, CertificateCancellation) are
    // migrated here. The create/update branch (map message → SaveCertificateOfOrigin → post-save declaration-submitted
    // check) is deferred and delivered together with the FluentValidation unit — because the legacy field validation
    // ALSO resolves the exporter / destination-country / org-unit / cert-to-update values the save consumes (the same
    // proxy checks produce them), so it cannot be wired faithfully without that unit. See the default-branch TODO.
    public async Task<CertificateOfOriginRequestFeedbackResponseDto> GetPC22802281CertificateOfOriginRequest(CertificateOfOriginRequestMessageDto request)
    {
        if (request.AgentRequest is null)
        {
            throw new RestValidationException(nameof(request.AgentRequest), "AgentRequest is required.");
        }

        // Legacy InternalGetPC...: an optional distributed lock (config IsNeedToLockCertificateOfOrigin), keyed by the
        // certificate id, serializes concurrent requests for the same certificate; released in a finally.
        var lockKey = request.AgentRequest.CertificateId;
        var needLock = !string.IsNullOrEmpty(lockKey) && await parametersUtil.Get<bool>("IsNeedToLockCertificateOfOrigin");
        if (!needLock)
        {
            var unlockedResult = await ProcessCertificateOfOriginRequest(request);
            return unlockedResult;
        }

        var lockState = await lockUtil.LockUntilAsync(lockKey!, TimeSpan.FromMinutes(5), nameof(GetPC22802281CertificateOfOriginRequest));
        if (!lockState.IsAcquired)
        {
            // Legacy: EMessages.ConcurrencyErrorTryAgain.
            throw new RestValidationException(nameof(request.AgentRequest.CertificateId), "The certificate is locked by another request; please try again.");
        }

        try
        {
            var result = await ProcessCertificateOfOriginRequest(request);
            return result;
        }
        finally
        {
            await lockUtil.SafeReleaseAsync(lockKey!, lockState);
        }
    }

    private async Task<CertificateOfOriginRequestFeedbackResponseDto> ProcessCertificateOfOriginRequest(CertificateOfOriginRequestMessageDto request)
    {
        var agentRequest = request.AgentRequest;
        var reasonCode = agentRequest.RequestReasonCode;

        // Legacy CheckRequestReasonAndGetSavedCertificate: the existing certificate the reason refers to. Not-found /
        // missing-id are accumulated as in-band request exceptions (legacy _requestExceptions), NOT thrown — the legacy
        // returns them on the feedback response, it does not 404 (mirrors GetCertificateRequestByGuid's in-band contract).
        var requestExceptions = new List<CertificateOfOriginExceptionDto>();

        // One per-request context spanning the whole message flow — carries the export-declaration-details cache so the
        // amendment guard, the per-reason declaration check and the post-save reconciliation share a single fetch.
        var context = new MessageValidationContext();

        // Legacy (Inner method, before the branch): for every reason except GetRequestStatus, block the request when the
        // linked export declaration is in an amendment process. Accumulated in-band (not thrown); for the create branch
        // the accumulated error blocks the save at the exception gate.
        if (reasonCode != (int)ERequestReason.GetRequestStatus)
        {
            await CheckDeclarationNotInAmendment(agentRequest.ExportDeclarationNum, context, requestExceptions);
        }

        CertificateOfOrigin? certificateToResponse;
        switch (reasonCode)
        {
            case (int)ERequestReason.GetRequestStatus:
                certificateToResponse = await GetSavedCertificateForMessage(agentRequest, requestExceptions);
                break;

            case (int)ERequestReason.CertificateCancellation:
                certificateToResponse = await GetSavedCertificateForMessage(agentRequest, requestExceptions);
                if (certificateToResponse != null)
                {
                    // Legacy ChecksIfThereIsDeclarationAssociatedWithTheCertificate: a certificate with a still-linked
                    // declaration cannot be cancelled — the declaration must be cancelled first.
                    var associatedDeclaration = await exportDealFileProxy.GetLeadDocumentByCertificateOfOriginId(certificateToResponse.Id);
                    if (associatedDeclaration != null)
                    {
                        requestExceptions.Add(BuildMessageException(EMessageCode.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate, associatedDeclaration.LeadDocumentTitle));
                    }
                }

                // Legacy: the exception gate (throw _requestExceptions) runs BEFORE the reason switch, so the cancel is
                // performed only when NOTHING accumulated — an amendment-linkage error, a not-found, or an associated
                // declaration all block it. Only cancel when the request is clean.
                if (certificateToResponse != null && requestExceptions.Count == 0)
                {
                    await CancelCertificateOfOriginFromMessage(certificateToResponse);
                }

                break;

            default:
                certificateToResponse = await ProcessCreateCertificateBranch(request, context, requestExceptions);
                break;
        }

        // Legacy: an unresolved certificate leaves the feedback body empty (the response header still carries the
        // accumulated exceptions). The read/cancel branches surface not-found this way rather than as a 404. All
        // exceptions — validation, not-found, and (once wired) declaration reconciliation — flow through the single
        // requestExceptions channel.
        var response = await BuildRequestFeedbackResponse(certificateToResponse, requestExceptions);
        return response;
    }

    // Legacy CheckCertificateNumber → GetCertificateOfOriginByExternalId: the latest certificate with this number.
    // Missing id → MustSendCertificateID; not found → CertificateDoesntExist. Both are accumulated as in-band request
    // exceptions (not thrown), matching the legacy _requestExceptions contract.
    private async Task<CertificateOfOrigin?> GetSavedCertificateForMessage(CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        if (string.IsNullOrEmpty(agentRequest.CertificateId))
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.MustSendCertificateID));
            return null;
        }

        var result = await DataLayer.GetLatestCertificateByNumberForFeedback(agentRequest.CertificateId);
        if (result == null)
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.CertificateDoesntExist, agentRequest.CertificateId));
        }

        return result;
    }

    // Legacy CancelCertificateOfOriginFromMessage: set the certificate to Cancelled with the cancel-from-message reason,
    // persist, and raise the user-cancelled event.
    private async Task CancelCertificateOfOriginFromMessage(CertificateOfOrigin certificate)
    {
        certificate.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Cancelled;

        // Legacy EMessages.CertificateOfOriginCancelFromMessage (from the UIMessage table). The generic resx/
        // ValidationMessages pipeline is still blocked repo-wide (BaseValidationMessages — see Program.cs), but this
        // specific message text is known, so it is set literally rather than deferred.
        certificate.RejectCancelReason = "התקבלה בקשה לביטול תעודה במסר";
        await DataLayer.CancelCertificateFromMessage(certificate.Id, certificate.RejectCancelReason, RequestMetadata.UserId ?? 0);

        var eventUtil = Resolve<IEventUtil>();
        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginUserCancelledCertificate, certificate.Id, certificate.OrganizationUnitId, certificate.RejectCancelReason);
    }

    // Legacy CreateCertificateOfOriginRequestFeedbackResponse: the feedback DTO + (create-branch) attachments. The
    // reconciliation exceptions (from the post-save declaration check) and the in-band request exceptions (not-found /
    // missing-id, accumulated above) are merged onto the response — the legacy returns them here, it does not throw.
    private async Task<CertificateOfOriginRequestFeedbackResponseDto> BuildRequestFeedbackResponse(CertificateOfOrigin? certificate, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        // A resolved certificate carries the full feedback; an unresolved one (not-found) leaves the feedback empty and
        // relies on the exceptions to convey the failure — the certificate id is then unknown (0).
        var feedback = certificate != null ? await BuildRequestFeedback(certificate) : new CertificateOfOriginRequestFeedbackDto();
        var response = new CertificateOfOriginRequestFeedbackResponseDto
        {
            ApplicationId = certificate?.Id ?? 0,
            Feedback = feedback,
            Exceptions = requestExceptions.Count > 0 ? requestExceptions : null,

            // Legacy: attachments are built (CreateAttachments → PrintCertificateOfOriginAndSaveAttachments) only for a
            // freshly-published certificate. The read/cancel reason codes handled here never carry attachments; the
            // attachment build belongs to the deferred create/save branch.
            Attachments = null,
        };
        return response;
    }

    // Legacy CreateCertificateOfOriginRequestFeedback: echo the certificate identity/status + the public query URL.
    private async Task<CertificateOfOriginRequestFeedbackDto> BuildRequestFeedback(CertificateOfOrigin certificate)
    {
        var urlTemplate = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        var queryUrl = certificate.Guid.HasValue
            ? string.Format(CultureInfo.InvariantCulture, urlTemplate ?? string.Empty, certificate.Guid)
            : null;

        return new CertificateOfOriginRequestFeedbackDto
        {
            InternalApplication = certificate.InternalApplication,
            CertificateId = certificate.CertificateNumber,
            CertificateOfOriginTypeCode = certificate.TypeId,
            CertificateOfOriginStatusCode = certificate.CertificateOfOriginStatusId,
            FeedbackRemark = certificate.FeedbackRemark,
            RejectCancelReason = certificate.RejectCancelReason,
            QueryUrl = queryUrl,
            RequestReasonCode = certificate.RequestReasonCode,

            // IssueDateIfReleased / IssueDateIfNotReleased: the legacy split is driven by IsDeclarationReleased, which is
            // computed only in the create/save + declaration-check flow (deferred). For the read/cancel branches it is
            // not computed, so both stay null — faithful (the legacy sets them only when IsDeclarationReleased.HasValue).
        };
    }

    #endregion

    #region GetCertificateRequestByGuid (Incoming / public-portal web query)

    // Incoming/portal WCF: GetCertificateRequestByGuid (GetPC_Web_9096_CertificateRequest) → CertificateOfOriginWebBL
    // .GetCertificateDetailForWeb. Certificate verification for the public portal, located by guid or by
    // number + issuing-date. The legacy in-band error contract is preserved: an invalid guid or no matching
    // certificate is returned as a populated ExceptionDescription (HTTP 200), not thrown as a 404 — the external
    // portal is unaffected.
    public async Task<CertificateOfOriginsResponseDto> GetCertificateRequestByGuid(CertificateOfOriginsRequestDto request)
    {
        if (request.CertificateOfOriginGuid != null && !Guid.TryParse(request.CertificateOfOriginGuid, out _))
        {
            return new CertificateOfOriginsResponseDto { ExceptionDescription = CertificateOfOriginsConsts.InvalidGuid };
        }

        var parameters = new DynamicParameters();
        parameters.Add("@Guid", request.CertificateOfOriginGuid != null ? Guid.Parse(request.CertificateOfOriginGuid) : (Guid?)null, DbType.Guid);
        parameters.Add("@CertificateOfOriginNumber", request.CertificateOfOriginNumber, DbType.String);
        parameters.Add("@IssuingDate", request.IssuingDate?.Date, DbType.Date);

        var certificate = await DataLayer.GetCertificateOfOriginDataForWebQuery(parameters);
        if (certificate == null)
        {
            return new CertificateOfOriginsResponseDto { ExceptionDescription = CertificateOfOriginsConsts.NoMatchingCertificate };
        }

        var response = await ConstructWebResponse(certificate);
        return response;
    }

    private async Task<CertificateOfOriginsResponseDto> ConstructWebResponse(CertificateOfOriginWebQueryDto certificate)
    {
        var response = new CertificateOfOriginsResponseDto
        {
            // The SP no longer resolves DocumentID (the cross-service Infrastructure.Docs_* JOIN was removed); it is
            // resolved from the Documents service instead — the newest document of type 329/461 attached to the
            // certificate (legacy SP: DED.EntityID=cert, EntityTypeID=12319, DD.TypeID IN (329,461), newest first).
            DocumentId = await ResolveWebQueryDocumentId(certificate.Id),
            CertificateNumber = certificate.CertificateNumber,
            QueryUrl = await GetQueryUrl(certificate.Guid),
            CertificateOfOriginDetails = await GetCertificateOfOriginDetails(certificate),
            CertificateOfOriginInvoiceDetails = await GetCertificateOfOriginInvoiceDetails(certificate),
        };
        return response;
    }

    // Legacy SP resolved DocumentID via Infrastructure.Docs_EntityDocument + Docs_Document: the newest document of
    // type 329/461 attached to the certificate (EntityTypeID = CertificateOfOrigin). Resolved here via the Documents
    // service (IDocumentsProxy.GetDocumentsByEntity; route is a rollout TODO(blocking)); 0 when there is no such document.
    private async Task<int> ResolveWebQueryDocumentId(int certificateId)
    {
        var documents = await documentsProxy.GetDocumentsByEntity(certificateId, (int)EEntityType.CertificateOfOrigin);
        var documentId = documents?
            .Where(document => CertificateOfOriginsConsts.WebQueryDocumentTypeIds.Contains(document.TypeId))
            .OrderByDescending(document => document.CreateDate)
            .Select(document => document.Id)
            .FirstOrDefault() ?? 0;
        return documentId;
    }

    private async Task<List<CertificateOfOriginWebInvoiceDetailDto>> GetCertificateOfOriginInvoiceDetails(CertificateOfOriginWebQueryDto certificateOfOrigin)
    {
        // Keep only the invoices that pass the (legacy) print filter, then resolve their currency codes in one batch.
        var printableInvoices = certificateOfOrigin.CertificateOfOriginInvoiceDetail
            .Where(invoice => IsInvoiceIncluded(certificateOfOrigin, invoice))
            .ToList();

        var currencyCodesById = await GetCurrencyCodes(
            printableInvoices.Where(i => i.CurrencyTypeId.HasValue).Select(i => i.CurrencyTypeId!.Value).ToList());

        var invoiceDetails = new List<CertificateOfOriginWebInvoiceDetailDto>();
        foreach (var invoice in printableInvoices)
        {
            var invoiceDetail = new CertificateOfOriginWebInvoiceDetailDto
            {
                InvoiceNumber = invoice.IsToPrint ? invoice.InvoiceNumber : string.Empty,
                InvoiceAmount = invoice.InvoiceAmount,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceGoodsDescription = invoice.InvoiceGoodsDescription,
            };

            // Legacy: SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode — resolved via ICurrencyTypeProxy
            // against the SystemTables microservice (no ILookupUtil type exists for CurrencyType).
            if (invoice.CurrencyTypeId.HasValue)
            {
                invoiceDetail.CurrencyCode = currencyCodesById.GetValueOrDefault(invoice.CurrencyTypeId.Value);
            }

            // LEGACY quirk (preserved, developer-confirmed 2026-07-28): the item-detail list was always
            // initialized and never populated — it is always empty.
            invoiceDetail.CertificateOfOriginItemDetails = [];

            invoiceDetails.Add(invoiceDetail);
        }

        return invoiceDetails;
    }

    private static bool IsInvoiceIncluded(CertificateOfOriginWebQueryDto certificateOfOrigin, CertificateOfOriginInvoiceDetailDto invoice)
    {
        // LEGACY operator-precedence quirk, preserved bug-for-bug (developer-confirmed 2026-07-28): because the
        // AND binds tighter than the OR in the original, the IsToPrint flag gates ONLY the MERCOSUR branch;
        // IsrCol, EURMED and EUR1 are always included regardless of IsToPrint. The inner parentheses below make
        // that original precedence explicit WITHOUT changing behavior (required by analyzer rule SA1408).
        return (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.IsrCol ||
                (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.MERCOSUR &&
                 invoice.IsToPrint)) ||
               (certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.EURMED ||
                certificateOfOrigin.TypeId == (int)ECertificateOfOriginType.EUR1);
    }

    private async Task<Dictionary<int, string?>> GetCurrencyCodes(List<int> currencyTypeIds)
    {
        if (currencyTypeIds.Count == 0)
        {
            return [];
        }

        var currencies = await currencyTypeProxy.GetCurrencyTypesByIds(currencyTypeIds.Distinct().ToList());
        if (currencies == null)
        {
            return [];
        }

        return currencies
            .GroupBy(c => c.Id)
            .ToDictionary(g => g.Key, g => g.First().CurrencyCode);
    }

    private async Task<List<FieldDataDto>> GetCertificateOfOriginDetails(CertificateOfOriginWebQueryDto certificate)
    {
        var fieldDataDtos = await BuildHeaderFields(certificate);

        foreach (var detail in certificate.CertificateOfOriginDetails)
        {
            var field = MapDetailField(certificate, detail);
            if (field != null)
            {
                fieldDataDtos.Add(field);
            }
        }

        return fieldDataDtos;
    }

    private async Task<List<FieldDataDto>> BuildHeaderFields(CertificateOfOriginWebQueryDto certificate)
    {
        var fieldDataDtos = new List<FieldDataDto>();

        var isRetrospective = certificate.RequestReasonCode == (int)ERequestReason.RetrospectiveCertificate;
        var isExportDecForPrint = !string.IsNullOrWhiteSpace(certificate.ExportDeclarationNumber) &&
            certificate.CertificateOfOriginDetails.Any(cood =>
                cood.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.IsExportDecForPrint &&
                bool.TryParse(cood.Value, out _));

        // Resolve the needed field labels once (was per-field SystemTablesUtil.GetCodeById<DataDictionaryField>).
        var labelFieldIds = new List<int>();
        if (isRetrospective)
        {
            labelFieldIds.Add(CertificateOfOriginsConsts.RequestReasonCodeFieldId);
        }

        if (certificate.CertificateIdToCancel.HasValue)
        {
            labelFieldIds.Add(CertificateOfOriginsConsts.CertificateIdToCancelFieldId);
        }

        if (isExportDecForPrint)
        {
            labelFieldIds.Add(CertificateOfOriginsConsts.ExportDeclarationNumberFieldId);
        }

        var labelsByFieldId = await GetFieldLabels(labelFieldIds);

        if (isRetrospective)
        {
            fieldDataDtos.Add(new FieldDataDto { Label = labelsByFieldId.GetValueOrDefault(CertificateOfOriginsConsts.RequestReasonCodeFieldId), Value = "Issued Retrospectively" });
        }

        if (certificate.CertificateIdToCancel.HasValue)
        {
            fieldDataDtos.Add(new FieldDataDto
            {
                Label = labelsByFieldId.GetValueOrDefault(CertificateOfOriginsConsts.CertificateIdToCancelFieldId),
                Value = $"Replacing certificate {certificate.CertificateIdToCancel.Value}"
            });
        }

        fieldDataDtos.Add(new FieldDataDto { Label = CertificateOfOriginsConsts.IssuingDateLabel, Value = certificate.IssuingDate });

        if (isExportDecForPrint)
        {
            fieldDataDtos.Add(new FieldDataDto { Label = labelsByFieldId.GetValueOrDefault(CertificateOfOriginsConsts.ExportDeclarationNumberFieldId), Value = certificate.ExportDeclarationNumber });
        }

        return fieldDataDtos;
    }

    private static FieldDataDto? MapDetailField(CertificateOfOriginWebQueryDto certificate, CertificateOfOriginWebDetailDto detail)
    {
        switch (detail.CertificateDetailsTypeCodeId)
        {
            case (int)ECertificateDetailsType.PlaceOfManufacture:
            case (int)ECertificateDetailsType.ZipCodeOfManufacture:
            case (int)ECertificateDetailsType.Observations:
            case (int)ECertificateDetailsType.IsDeclaredByManufacturer:
            case (int)ECertificateDetailsType.IsDeclaredByExporter:
            case (int)ECertificateDetailsType.ExporterAddress:
            case (int)ECertificateDetailsType.ExporterName:
            case (int)ECertificateDetailsType.CountryOfDeclaration:
            case (int)ECertificateDetailsType.DestinationCountry:
            case (int)ECertificateDetailsType.ExporterCountry:
            case (int)ECertificateDetailsType.OriginCountry:
            case (int)ECertificateDetailsType.CustomsHouse:
            case (int)ECertificateDetailsType.ExporterId:
            case (int)ECertificateDetailsType.TradeAgreementCountry1:
            case (int)ECertificateDetailsType.TradeAgreementCountry2:
            case (int)ECertificateDetailsType.TradeAgreementGroupOfCountries:
            case (int)ECertificateDetailsType.ConsigneeRemarks:
            case (int)ECertificateDetailsType.OriginGroupOfCountries:
            case (int)ECertificateDetailsType.DestinationGroupOfCountries:
            case (int)ECertificateDetailsType.Transport:
            case (int)ECertificateDetailsType.IssuingCountry:
            case (int)ECertificateDetailsType.CityOfDeclaration:
            case (int)ECertificateDetailsType.PortOfShipment:
                return PrintOutField(detail);
            case (int)ECertificateDetailsType.DateOfDeclaration:
                return MapDateOfDeclarationField(detail);
            case (int)ECertificateDetailsType.ConsigneeName:
            case (int)ECertificateDetailsType.ConsigneeAddress:
            case (int)ECertificateDetailsType.ConsigneeCountry:
                return MapConsigneeField(certificate, detail);
            default:
                return null;
        }
    }

    private static FieldDataDto? MapDateOfDeclarationField(CertificateOfOriginWebDetailDto detail)
    {
        if (!string.IsNullOrWhiteSpace(detail.Value) &&
            DateTime.TryParse(detail.Value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateOfDeclaration))
        {
            return new FieldDataDto
            {
                Label = detail.CertificateDetailsTypeCode?.EnglishName,
                Value = dateOfDeclaration.ToString("dd MMMM yyyy", new CultureInfo("en-US"))
            };
        }

        return null;
    }

    private static FieldDataDto? MapConsigneeField(CertificateOfOriginWebQueryDto certificate, CertificateOfOriginWebDetailDto detail)
    {
        var isConsigneeForPrint = certificate.CertificateOfOriginDetails.Any(cood =>
            cood.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.IsConsigneeForPrint &&
            bool.TryParse(cood.Value, out _));
        if (!isConsigneeForPrint)
        {
            return null;
        }

        if (certificate.TypeId == (int)ECertificateOfOriginType.EUR1 ||
            certificate.TypeId == (int)ECertificateOfOriginType.EURMED)
        {
            // LEGACY quirk (preserved, developer-confirmed 2026-07-28): result set 5 never returns an IsToPrint
            // column, so this is always false — Consignee fields for EUR1/EURMED are never printed.
            return detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeIsToPrint == true
                ? PrintOutField(detail)
                : null;
        }

        return PrintOutField(detail);
    }

    private static FieldDataDto PrintOutField(CertificateOfOriginWebDetailDto detail)
    {
        return new FieldDataDto
        {
            Label = detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeEnglishName,
            Value = detail.CertificateOfOriginWebPrintOut?.CertificateDetailsTypeValue
        };
    }

    private async Task<Dictionary<int, string?>> GetFieldLabels(List<int> fieldIds)
    {
        if (fieldIds.Count == 0)
        {
            return [];
        }

        var fields = await dataDictionaryFieldProxy.GetDataDictionaryFieldsByIds(fieldIds.Distinct().ToList());
        if (fields == null)
        {
            return [];
        }

        return fields
            .GroupBy(f => f.Id)
            .ToDictionary(g => g.Key, g => g.First().EnglishName);
    }

    private async Task<string> GetQueryUrl(Guid? guid)
    {
        if (!guid.HasValue)
        {
            return string.Empty;
        }

        // CertificateOfOriginQueryURL — a format string with {0} for the guid (was Configuration.GetConfig in the
        // legacy). Seeded in the local Infrastructure.Parameters table (Active, Level 1).
        var url = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        return string.Format(url, guid.Value.ToString());
    }

    #endregion

    public async Task<VirtualEntityDto> Convert(ConnectedEntityDto connectedEntity)
    {
        // ESB/EAI Convert: resolve the connected-entity key (the certificate number) into a generic entity link.
        // Reuses the #7 filter search; a missing certificate owns the 404 contract (legacy threw not-exist).
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = connectedEntity.EntityIdKey1 };
        var certificate = (await GetCertificateOfOriginsByFilter(filter)).FirstOrDefault()
            ?? throw new RestNotFoundException();

        var result = new VirtualEntityDto
        {
            Id = certificate.Id,
            Title = certificate.Name,
            EntityType = (int)EEntityType.CertificateOfOrigin,
            CustomerId = certificate.CustomesAgentId,
        };
        return result;
    }

    public async Task<bool> LoadDataFromExportDeclaration(LoadDataFromExportDeclarationRequestDto request)
    {
        // Guard: without a lead-document id or an export-declaration number there is nothing to look up.
        if (request.LeadDocumentId is null && string.IsNullOrEmpty(request.ExportDeclarationNumber))
        {
            return false;
        }

        var details = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(
            request.LeadDocumentId, request.ExportDeclarationNumber);

        // The legacy set IsDeclarationReleased/IsCargoExitedOfCustomsRegulation back on the entity (by-ref) and
        // returned this computed flag; over REST only the flag is returned (developer decision 2026-07-27). It is
        // true only when the cargo has exited customs regulation and the request is not a retrospective certificate.
        var isCargoExited = details?.IsCargoExitedOfCustomsRegulation ?? false;
        return isCargoExited && request.RequestReasonCode != (int)ERequestReason.RetrospectiveCertificate;
    }

    public async Task<int> GetCertificateOfOriginID(string certificateNumber)
    {
        // route-style alternate key → not-found owns the 404 contract (RestNotFoundException)
        var result = await DataLayer.GetCertificateOfOriginIdByNumber(certificateNumber)
            ?? throw new RestNotFoundException();
        return result;
    }

    #region LEGACY_WCF
#pragma warning disable S125 // migration convention: preserve the original WCF body as a reference comment

    // Original WCF (CertificateOfOriginsExternalService.InternalGetCertificateOfOriginID):
    //
    // public int? InternalGetCertificateOfOriginID(string certificateNumber)
    // {
    //     using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
    //     {
    //         var certificateOfOrigin = uow.Repository.GetQuery<CertificateOfOrigin>()
    //             .OrderByDescending(c => c.CreateDate)
    //             .FirstOrDefault(c => c.CertificateNumber == certificateNumber);
    //         return certificateOfOrigin?.ID;
    //     }
    // }
#pragma warning restore S125
    #endregion

    public async Task<List<GoodsItemCerificateDto>> GetGoodsItemCerificateDTO(List<GoodsItemCerificateDto> goodsItemCerificateDTOs)
    {
        foreach (var item in goodsItemCerificateDTOs)
        {
            if (item.CertificateNumber != null)
            {
                item.CertificateOfOriginId = await DataLayer.GetCertificateOfOriginIdByNumber(item.CertificateNumber);
            }
        }

        return goodsItemCerificateDTOs;
    }

    public async Task<CertificateOfOriginResultDto?> IsCertificateOfOriginByExternalIdExist(string certificateOfOriginExternalId)
    {
        var filter = new CertificateOfOriginFilterDto { CertificateNumber = certificateOfOriginExternalId };
        var certificates = await GetCertificateOfOriginsByFilter(filter);
        var result = certificates.FirstOrDefault();
        return result;
    }

    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var parameters = BuildParameterForProcedure(filter);
        var result = await DataLayer.GetCertificateOfOriginsByFilter(parameters);
        await FillCustomersInformation(result);
        return result;
    }

    private static DynamicParameters BuildParameterForProcedure(CertificateOfOriginFilterDto filter)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateNumber", filter.CertificateNumber, DbType.String);
        parameters.Add("@CertificateOfOriginStatusID", filter.CertificateOfOriginStatusId, DbType.Int32);
        parameters.Add("@CertificateOfOriginTypeID", filter.CertificateOfOriginTypeId, DbType.Int32);
        parameters.Add("@CustomsAgentID", filter.CustomsAgentId, DbType.Int32);
        parameters.Add("@CustomsHouseID", filter.CustomsHouseId, DbType.Int32);
        parameters.Add("@DestinationCountry", filter.DestinationCountry, DbType.Int32);
        parameters.Add("@ExportDeclarationID", filter.ExportDeclarationId, DbType.Int32);
        parameters.Add("@ExportDeclarationNum", filter.ExportDeclarationNum, DbType.String);
        parameters.Add("@ExporterCustomerID", filter.ExporterCustomerId, DbType.Int32);
        parameters.Add("@FromIssuingDate", filter.FromIssuingDate, DbType.DateTime);
        parameters.Add("@ToIssuingDate", filter.ToIssuingDate, DbType.DateTime);
        parameters.Add("@FromRequestDate", filter.FromRequestDate, DbType.DateTime);
        parameters.Add("@ToRequestDate", filter.ToRequestDate, DbType.DateTime);
        parameters.Add("@RequestReasonID", filter.RequestReasonId, DbType.Int32);
        parameters.Add("@VersionNumber", filter.VersionNumber, DbType.Int32);
        parameters.Add("@IsLastVersion", filter.IsLastVersion, DbType.Boolean);
        return parameters;
    }

    private async Task FillCustomersInformation(List<CertificateOfOriginResultDto> certificates)
    {
        if (certificates.Count == 0)
        {
            return;
        }

        var customerIds = certificates.Select(c => c.ExporterId)
            .Concat(certificates.Select(c => c.CustomesAgentId))
            .Distinct()
            .ToList();
        var customers = await customerProxy.GetCustomersByIds(customerIds);
        if (customers == null)
        {
            return;
        }

        var customersById = customers.ToDictionary(c => c.Id);
        foreach (var certificate in certificates)
        {
            if (customersById.TryGetValue(certificate.ExporterId, out var exporter))
            {
                certificate.ExporterTitle = exporter.Name;
                certificate.ExporterExternalIdNum = exporter.ExternalIdNum;
            }

            if (customersById.TryGetValue(certificate.CustomesAgentId, out var agent))
            {
                certificate.CustomesAgentTitle = agent.Name;
                certificate.CustomesAgentExternalIdNum = agent.ExternalIdNum;
            }
        }
    }

    #region SaveCertificateOfOriginAttachments (External / print-and-save)

    // External WCF: SaveCertificateOfOriginAttachments(args) — saves the generated certificate template(s) as
    // attachments on the certificate. For each template it clears the certificate's existing documents and uploads
    // the new one (via IDocumentUtil). The legacy loop structure is preserved bug-for-bug — the fetch+delete of the
    // existing documents sits INSIDE the loop, so each template replaces whatever is currently attached.
    public async Task<bool> SaveCertificateOfOriginAttachments(SaveCertificateAttachmentsArgsDto request)
    {
        // Legacy: SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>(CertificateTypeID).Name. No ILookupUtil
        // type exists for this SystemTable, so the certificate-type display name is taken from the
        // ECertificateOfOriginType enum instead (developer decision 2026-08-02).
        var certificateTypeName = GetCertificateTypeName(request.CertificateTypeId);

        var isDraft = request.CertificateRequestReasonCode == (int)ERequestReason.Draft ||
                      request.AdditionalInfo == CertificateOfOriginsConsts.IsDraftSentinel;
        var title = $"{certificateTypeName} - {(isDraft ? CertificateOfOriginsConsts.DraftLabel : CertificateOfOriginsConsts.FinalLabel)}";

        // Legacy: UserUtil.Current.OrganizationUnitID — the current user's organization unit. RequestMetadata.User is
        // not populated in this service's request pipeline (only the flat RequestMetadata.UserId is, from the
        // CC-USER-ID header), so the org unit is resolved from the Users microservice by that user id.
        var organizationUnitId = await GetCurrentUserOrganizationUnitId();

        var documentUtil = Resolve<IDocumentUtil>();

        // Legacy: FileName = string.Format(CertificateName, type, number) — a Hebrew filename with spaces. The .NET10
        // IDocumentUtil validates the filename and rejects the legacy chars, so it is sanitized against the util's own
        // invalid-char set (the human-readable Hebrew name is preserved in the Title, which is not validated).
        var fileName = SanitizeFileName(documentUtil, string.Format(CultureInfo.CurrentCulture, CertificateOfOriginsConsts.CertificateNameFormat, certificateTypeName, request.CertificateNumber));

        foreach (var certificateTemplate in request.CertificatesTemplates)
        {
            // Replace the certificate's currently-attached documents before uploading (legacy: GetDocumentsByEntitySync
            // + DeleteDocument, both inside the loop).
            var existingDocuments = await documentsProxy.GetDocumentsByEntity(request.CertificateId, (int)EEntityType.CertificateOfOrigin);
            if (existingDocuments != null && existingDocuments.Count > 0)
            {
                var entity = new VirtualEntityDto { Id = request.CertificateId, EntityType = (int)EEntityType.CertificateOfOrigin };
                await documentsProxy.DeleteDocuments(existingDocuments.Select(document => document.Id).ToList(), entity);
            }

            var documentBuilder = documentUtil.CreateDocumentBuilder()
                .WithFileName(fileName)
                .WithTitle(title)
                .WithContent(certificateTemplate.Content)
                .WithTypeId((DocumentType)certificateTemplate.DocumentTypeId)
                .WithEntityId(request.CertificateId)
                .WithEntityTypeId((int)EEntityType.CertificateOfOrigin)
                .WithOrganizationUnitId(organizationUnitId);

            // Legacy: only the ExportCertificateOfOrigin document type carried the certificate-number additional field.
            if (certificateTemplate.DocumentTypeId == CertificateOfOriginsConsts.ExportCertificateOfOriginDocumentTypeId)
            {
                documentBuilder.AddAdditionalFields(field => field
                    .WithId(CertificateOfOriginsConsts.CertificateNumberAdditionalFieldId)
                    .WithValue(request.CertificateNumber));
            }

            await documentUtil.UploadDocument(documentBuilder.Build());
        }

        return true;
    }

    // The current user's organization unit (legacy UserUtil.Current.OrganizationUnitID). RequestMetadata.User is not
    // populated in this pipeline, so it is resolved from the Users microservice by RequestMetadata.UserId (which IS
    // populated, from the CC-USER-ID header). Falls back to 0 when there is no current user / the user is not found.
    private async Task<int> GetCurrentUserOrganizationUnitId()
    {
        if (RequestMetadata.UserId is not int userId)
        {
            return 0;
        }

        var users = await userProxy.GetUsersByIds([userId]);
        return users?.FirstOrDefault()?.OrganizationUnit ?? 0;
    }

    // The certificate-type display name (replaces SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>.Name)
    // — taken from the ECertificateOfOriginType [Display(Name)] attribute, falling back to the member name.
    private static string GetCertificateTypeName(int certificateTypeId)
    {
        var certificateType = (ECertificateOfOriginType)certificateTypeId;
        var memberName = certificateType.ToString();
        var member = typeof(ECertificateOfOriginType).GetMember(memberName).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? memberName;
    }

    // The .NET10 IDocumentUtil validates the filename and rejects chars the legacy Documents service accepted
    // (the legacy filename is Hebrew with spaces). Replace every char the util reports as invalid with '_' so the
    // upload succeeds; the readable Hebrew name lives on the document Title, which is not validated.
    private static string SanitizeFileName(IDocumentUtil documentUtil, string fileName)
    {
        var invalidChars = documentUtil.GetInvalidFilenameChars().ToHashSet();
        if (invalidChars.Count == 0)
        {
            return fileName;
        }

        var builder = new StringBuilder(fileName.Length);
        foreach (var character in fileName)
        {
            builder.Append(invalidChars.Contains(character) ? '_' : character);
        }

        return builder.ToString();
    }

    #endregion

    // Internal WCF: SaveCertificateOfOrigin(certificate) — the certificate-of-origin save. Faithful CORE (developer
    // decision 2026-08-06 — core with mock proxies + TODO for the un-stood-up services): supersede the previous
    // version on a new instance, validate/enrich the detail rows, generate the QR on publish, upsert the certificate
    // + diff-merge its details, link the DealFile lead document, raise the status-change events, send the request-
    // feedback message, and on publish generate the template attachments + handle replacement. Returns the full
    // re-read graph (GetCertificateOfOriginById) — the established save-return convention.
    // TODO(migration): country-group + international-site id→name (no ILookupUtil type — need a SystemTables proxy) and
    // the exact EAI feedback-message mapping are deferred (inline TODOs).
    public async Task<CertificateOfOriginDto> SaveCertificateOfOrigin(SaveCertificateOfOriginRequestDto request)
    {
        var userId = RequestMetadata.UserId ?? 0;
        var eventUtil = Resolve<IEventUtil>();

        var entity = BuildCertificateEntity(request);
        var details = BuildDetails(request);
        var isNewInstance = entity.Id == 0; // captured before the save assigns the real id

        // New instance (not a cancellation): supersede the latest existing certificate with the same number.
        var replacementOldId = 0;
        var previousCertificateId = 0;
        if (isNewInstance && entity.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Cancelled)
        {
            (replacementOldId, previousCertificateId) = await SupersedePreviousVersion(entity);
        }

        byte[]? qrCodeToUpload = null;
        if (entity.RequestReasonCode != (int)ERequestReason.CertificateCancellation)
        {
            // Generate the QR (stamps QrImage + Guid onto the entity) BEFORE the save so they persist with the upsert;
            // the returned bytes are uploaded as a document AFTER the save, once the certificate id is assigned.
            qrCodeToUpload = await CreateQrCodeIfNeeded(entity);
            await EnrichAndValidateDetails(entity, details);
        }

        if (entity.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.PendingRelease or (int)ECertificateOfOriginStatus.Published)
        {
            entity.ApproveUserId = userId;
        }

        // Persist (upsert + diff-merge details + diff-merge the invoice/item graph when supplied by the incoming-message
        // create branch; the SPA save path sends no invoices).
        entity.Id = await DataLayer.SaveCertificateOfOrigin(entity, details, request.CertificateOfOriginInvoiceDetails, userId);

        // Upload the QR document now that the save assigned the certificate id — the document is linked to the real id
        // (for a brand-new certificate published in one save, entity.Id was still 0 during generation) and QrCodePath is
        // persisted. Legacy uploaded before its single UoW commit; the migration's per-mutation writes move it after the
        // upsert so the id-linked upload sees the assigned id.
        if (qrCodeToUpload is not null)
        {
            await UploadQrCodeDocument(entity, qrCodeToUpload, userId);
        }

        // Supersede events — raised here (not in SupersedePreviousVersion) because ApplicationCorrected references the
        // new certificate's id, which is only assigned by the save above. Legacy raised both when a previous existed.
        if (previousCertificateId != 0)
        {
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginApplicationCorrected, entity.Id, entity.OrganizationUnitId, null);
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginUserCancelledCertificate, previousCertificateId, entity.OrganizationUnitId, null);
        }

        // Link the DealFile lead document (repoint from the superseded certificate to this one).
        await LinkLeadDocument(entity, replacementOldId, userId);

        // Status-change / remarks-change side effects. Legacy CheckIfStatusChangedAndHandleChanges: a NEW instance has
        // no tracked original status, so its side effects fire only when the status is Received; an existing instance
        // compares against the original status.
        var isStatusChanged = isNewInstance
            ? entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Received
            : entity.CertificateOfOriginStatusId != request.OriginalCertificateOfOriginStatusId;
        var isRemarksChanged = !string.Equals(entity.FeedbackRemark, request.OriginalFeedbackRemark, StringComparison.Ordinal);

        if (isStatusChanged)
        {
            await RaiseStatusEvents(entity, eventUtil);
        }

        if ((isStatusChanged && entity.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published) || isRemarksChanged)
        {
            await SendRequestFeedback(entity);
        }

        if (isStatusChanged && entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Published)
        {
            await PublishAttachments(entity, eventUtil, userId);
            if (entity.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
            {
                await HandleCertificateReplacement(entity, eventUtil);
            }
        }

        return await GetCertificateOfOriginById(entity.Id);
    }

    private static CertificateOfOrigin BuildCertificateEntity(SaveCertificateOfOriginRequestDto r)
    {
        return new CertificateOfOrigin
        {
            Id = r.Id,
            TypeId = r.TypeId,
            Title = r.Title ?? string.Empty,
            State = r.State,
            TimeStamp = r.TimeStamp ?? [],
            OrganizationUnitId = r.OrganizationUnitId,
            CustomerId = r.CustomerId,
            CreateCustomerId = r.CreateCustomerId,
            UpdateCustomerId = r.UpdateCustomerId,
            LeadDocumentId = r.LeadDocumentId,
            CertificateIdToCancel = r.CertificateIdToCancel,
            CertificateNumber = r.CertificateNumber ?? string.Empty,
            CertificateOfOriginStatusId = r.CertificateOfOriginStatusId,
            DestinationCountry = r.DestinationCountry,
            FeedbackRemark = r.FeedbackRemark,
            InternalApplication = r.InternalApplication,
            IssuingDate = r.IssuingDate,
            RejectCancelReason = r.RejectCancelReason,
            ReplacementReason = r.ReplacementReason,
            RequestReasonCode = r.RequestReasonCode,
            ExportDeclarationNumber = r.ExportDeclarationNumber,
            CertificateToReplaceInImport = r.CertificateToReplaceInImport,
            Guid = r.Guid,
            QrCodePath = r.QrCodePath,
            QrImage = r.QrImage,
            IsAttachedList = r.IsAttachedList,
            InSufficentworkingInd = r.InSufficentworkingInd,
            InsufficentWorkingText = r.InsufficentWorkingText,
            VersionNumber = r.VersionNumber,
            IsLastVersion = r.IsLastVersion,
            ApproveUserId = r.ApproveUserId,
            IsInPublishingProcess = r.IsInPublishingProcess,
        };
    }

    private static List<CertificateOfOriginDetails> BuildDetails(SaveCertificateOfOriginRequestDto r)
    {
        return r.CertificateOfOriginDetails
            .Select(d => new CertificateOfOriginDetails
            {
                Id = d.Id,
                CertificateOfOriginId = d.CertificateOfOriginId,
                CertificateDetailsTypeCodeId = d.CertificateDetailsTypeCodeId,
                Value = d.Value,
                DisplayedValue = d.DisplayedValue,
            })
            .ToList();
    }

    // Legacy: on a new instance, cancel the latest existing certificate with the same number, bump the version, and
    // raise the application-corrected + user-cancelled events. Returns the superseded certificate id (0 if none).
    // Returns (replacementOldId — the previous version to relink the lead document from, 0 when it was already
    // cancelled; previousCertificateId — the previous version's id, 0 when there was none). The supersede events are
    // NOT raised here: ApplicationCorrected references the new certificate's id, which is only assigned by the save
    // that runs after this method — so the caller raises them post-save.
    private async Task<(int ReplacementOldId, int PreviousCertificateId)> SupersedePreviousVersion(CertificateOfOrigin entity)
    {
        var previous = await DataLayer.GetLatestCertificateByNumber(entity.CertificateNumber);
        if (previous is null)
        {
            entity.VersionNumber = 1;
            entity.IsLastVersion = true;
            return (0, 0);
        }

        // Legacy cancels the previous version + clears its IsLastVersion UNCONDITIONALLY; only the replacement-id link
        // is guarded by "not already cancelled". Guarding the whole cancel would leave an already-cancelled previous
        // still flagged IsLastVersion=true → two last-version rows for the number.
        // "\n<update received>" — the legacy appended EServerTerms.CertificateUpdateRecived.
        await DataLayer.CancelPreviousCertificate(previous.Id, Environment.NewLine + CertificateOfOriginsConsts.CertificateUpdateReceived, RequestMetadata.UserId ?? 0);

        entity.VersionNumber = previous.VersionNumber + 1;
        entity.IsLastVersion = true;

        var replacementOldId = previous.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Cancelled ? previous.Id : 0;
        return (replacementOldId, previous.Id);
    }

    // Legacy CreateQRCodeIfNeededAndUpload (generation half) — on publish (and only when no QR path yet), generate the
    // QR from the certificate's query URL and stamp QrImage + Guid so they persist with the main upsert. The document
    // upload is split out into UploadQrCodeDocument, which runs AFTER the save: the QR document is linked to the
    // certificate id, and that id is only assigned by the save (a brand-new certificate published in one save still has
    // Id == 0 here — linking now would attach the QR to certificate id 0). Returns the freshly-generated QR bytes to
    // upload post-save, or null when nothing was created (path already set / not Published / empty CreateQrCode result).
    private async Task<byte[]?> CreateQrCodeIfNeeded(CertificateOfOrigin entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.QrCodePath) || entity.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published)
        {
            return null;
        }

        var urlTemplate = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        entity.Guid = System.Guid.NewGuid();
        var url = string.Format(CultureInfo.InvariantCulture, urlTemplate ?? string.Empty, entity.Guid);
        var qrBytes = await commonServicesProxy.CreateQrCode(url);
        entity.QrImage = qrBytes;

        return qrBytes is { Length: > 0 } ? qrBytes : null;
    }

    // Legacy CreateQRCodeIfNeededAndUpload (upload half) — upload the generated QR image as a document (legacy
    // DocumentRepositoryUtil.UploadFile → GetDocumentFile) linked to the SAVED certificate, keep the returned resource
    // path on QrCodePath, and persist it. Runs AFTER the main upsert so the document is linked to the real certificate
    // id (not 0); QrImage + Guid were persisted by the upsert, but QrCodePath — unknown until this upload — needs its
    // own write.
    private async Task UploadQrCodeDocument(CertificateOfOrigin entity, byte[] qrBytes, int userId)
    {
        var documentUtil = Resolve<IDocumentUtil>();
        var organizationUnitId = await GetCurrentUserOrganizationUnitId();
        var fileName = SanitizeFileName(documentUtil, entity.CertificateNumber + ".jpg");
        var document = documentUtil.CreateDocumentBuilder()
            .WithFileName(fileName)
            .WithTitle(entity.CertificateNumber)
            .WithContent(qrBytes)
            .WithTypeId(DocumentType.Other)
            .WithEntityId(entity.Id)
            .WithEntityTypeId((int)EEntityType.CertificateOfOrigin)
            .WithOrganizationUnitId(organizationUnitId)
            .Build();
        var response = await documentUtil.UploadDocument(document);
        entity.QrCodePath = response.ExternalId;
        await DataLayer.UpdateCertificateQrCodePath(entity.Id, entity.QrCodePath, userId);
    }

    // Country detail types whose id → English name is resolved for display (legacy CheckSpecificField country cases).
    private static readonly HashSet<int> CountryDetailTypes =
    [
        (int)ECertificateDetailsType.ExporterCountry, (int)ECertificateDetailsType.TradeAgreementCountry1,
        (int)ECertificateDetailsType.TradeAgreementCountry2, (int)ECertificateDetailsType.ConsigneeCountry,
        (int)ECertificateDetailsType.OriginCountry, (int)ECertificateDetailsType.DestinationCountry,
        (int)ECertificateDetailsType.CumulationCountry, (int)ECertificateDetailsType.IssuingCountry,
        (int)ECertificateDetailsType.CountryOfDeclaration, (int)ECertificateDetailsType.ExportCountry,
        (int)ECertificateDetailsType.TransirCountry,
    ];

    // The subset of country types whose value is also checked for trade-agreement membership (legacy
    // CheckIfCountryIsInTradeAgreement / CheckAgreementFirstCountry).
    private static readonly HashSet<int> TradeAgreementCountryDetailTypes =
    [
        (int)ECertificateDetailsType.TradeAgreementCountry1, (int)ECertificateDetailsType.TradeAgreementCountry2,
        (int)ECertificateDetailsType.ConsigneeCountry, (int)ECertificateDetailsType.OriginCountry,
        (int)ECertificateDetailsType.CumulationCountry, (int)ECertificateDetailsType.DestinationCountry,
    ];

    private static readonly HashSet<int> CityDetailTypes =
    [
        (int)ECertificateDetailsType.CityOfDeclaration, (int)ECertificateDetailsType.PlaceOfManufacture,
    ];

    // Legacy CheckSpecificField (reduced core): the proxy-backed field validations (exporter existence via Customers,
    // trade-agreement membership via CustomsBook, customs-house via OrgUnit) + the SystemTables id→name display
    // enrichment for country + city detail types (via ILookupUtil). Text fields pass through.
    // TODO(migration): country-group + international-site id→name have no ILookupUtil type — they need a SystemTables
    // proxy (rollout); the not-in-system / not-in-agreement validation exceptions + date/format checks are deferred (resx).
    private async Task EnrichAndValidateDetails(CertificateOfOrigin entity, List<CertificateOfOriginDetails> details)
    {
        foreach (var detail in details)
        {
            var value = detail.Value;
            var typeId = detail.CertificateDetailsTypeCodeId;
            if (typeId == (int)ECertificateDetailsType.ExporterId)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var exporterId = await customerProxy.GetCustomerIdByExternalId(value);
                    if (exporterId.GetValueOrDefault() != 0)
                    {
                        detail.Value = exporterId.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }

                detail.DisplayedValue = detail.Value;
            }
            else if (typeId == (int)ECertificateDetailsType.CustomsHouse)
            {
                if (int.TryParse(value, out var orgUnitId))
                {
                    await organizationUnitProxy.IsOrganizationUnitCustomsHouse(orgUnitId);
                }

                detail.DisplayedValue = value;
            }
            else if (CountryDetailTypes.Contains(typeId) && int.TryParse(value, out var countryId))
            {
                if (TradeAgreementCountryDetailTypes.Contains(typeId))
                {
                    await customsBookProxy.IsTradeAgreementForCountry(entity.TypeId, countryId, false);
                }

                var country = await lookupUtil.Get<Lookup.Country>(countryId);
                detail.DisplayedValue = country?.EnglishName ?? value;
            }
            else if (CityDetailTypes.Contains(typeId) && int.TryParse(value, out var cityId))
            {
                var city = await lookupUtil.Get<Lookup.City>(cityId);
                detail.DisplayedValue = city?.EnglishName ?? value;
            }
            else
            {
                // Country-group + international-site id→name enrichment lands here for now (no ILookupUtil type; needs a
                // SystemTables proxy — rollout TODO), as do all text fields — displayed as-is.
                detail.DisplayedValue = value;
            }
        }
    }

    // Legacy: repoint the DealFile lead document to this certificate; backfill LeadDocumentId/ExportDeclarationNumber
    // when the certificate has none.
    private async Task LinkLeadDocument(CertificateOfOrigin entity, int replacementOldId, int userId)
    {
        var oldId = replacementOldId != 0 ? replacementOldId : entity.Id;
        var leadDocument = await exportDealFileProxy.GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(oldId, entity.Id);
        if (leadDocument is null)
        {
            return;
        }

        if (string.IsNullOrEmpty(entity.ExportDeclarationNumber))
        {
            entity.LeadDocumentId = leadDocument.LeadDocumentId;
            entity.ExportDeclarationNumber = leadDocument.LeadDocumentTitle;

            // The backfill is stamped after the main upsert (this method needs the new certificate id), so it needs its
            // own explicit write to persist. TODO(migration): the title-mismatch validation + CheckDeclarationStatus are deferred.
            await DataLayer.UpdateCertificateDeclarationLink(entity.Id, entity.LeadDocumentId, entity.ExportDeclarationNumber, userId);
        }
    }

    // Legacy RaiseCertificateOfOriginEvents: the status-driven event for the new status, plus the "new certificate
    // created" secondary event on the Received / DeclarationMatch statuses. The legacy DeclarationMismatch "assessor
    // decision" is a no-op (empty in the legacy), so nothing is raised there.
    private async Task RaiseStatusEvents(CertificateOfOrigin entity, IEventUtil eventUtil)
    {
        int? specificEvent = entity.CertificateOfOriginStatusId switch
        {
            (int)ECertificateOfOriginStatus.Received => (int)EEventType.CertificateOfOriginApplicationReceived,
            (int)ECertificateOfOriginStatus.Rejected => (int)EEventType.CertificateOfOriginUserDeniedCertificate,
            (int)ECertificateOfOriginStatus.Cancelled => (int)EEventType.CertificateOfOriginUserCancelledCertificate,
            (int)ECertificateOfOriginStatus.PendingRelease => (int)EEventType.CertificateOfOriginUserApprovedCertificate,
            (int)ECertificateOfOriginStatus.DeclarationMatch => (int)EEventType.CertificateOfOriginCertificateMatchDeclaration,
            (int)ECertificateOfOriginStatus.DeclarationMismatch => (int)EEventType.CertificateOfOriginCertificateDeclarationMismatch,
            _ => null,
        };
        if (specificEvent is not null)
        {
            await RaiseCertificateEvent(eventUtil, specificEvent.Value, entity.Id, entity.OrganizationUnitId, null);
        }

        await RaiseNewCertificateOfOriginCreatedEvent(entity, eventUtil);
    }

    // Legacy RaiseNewCertificateOfOriginCreatedEvent — on Received / DeclarationMatch, open the "new certificate check"
    // event (RaiseTaskNewCertificateOfOriginCheck → assessor as preferred user), skipped for the transient reasons and
    // only when the export-declaration integration is off.
    private async Task RaiseNewCertificateOfOriginCreatedEvent(CertificateOfOrigin entity, IEventUtil eventUtil)
    {
        if (entity.CertificateOfOriginStatusId is not ((int)ECertificateOfOriginStatus.Received or (int)ECertificateOfOriginStatus.DeclarationMatch))
        {
            return;
        }

        if (entity.RequestReasonCode is (int)ERequestReason.GetRequestStatus or (int)ERequestReason.CertificateCancellation
            or (int)ERequestReason.EmptyCertificate or (int)ERequestReason.Draft)
        {
            return;
        }

        var isExportDeclarationActive = await parametersUtil.Get<bool>("IsExportDeclarationActive");
        if (isExportDeclarationActive)
        {
            return;
        }

        await RaiseCertificatePreferredAssessorEvent(entity, eventUtil, (int)EEventType.CertificateOfOriginNewCertificateOfOriginCreated);
    }

    // Legacy SendRequestFeedback — the certificate request-feedback EAI message to the creating customer.
    // TODO(migration): the exact PC_NG_2281_MSG02 → SendMessageDto field mapping is deferred (message type + params).
    private async Task SendRequestFeedback(CertificateOfOrigin entity)
    {
        var message = new SendMessageDto
        {
            RelatedEntity = new VirtualEntityDto { Id = entity.Id, EntityType = (int)EEntityType.CertificateOfOrigin, CustomerId = entity.CreateCustomerId },
            MessageTypeId = 0, // TODO(blocking): map the real EMessageTypes value for the certificate request feedback.
            MessageParameters = [entity.CertificateNumber],
            UserIdToSendMessage = entity.CreateCustomerId,
        };
        await messageManagementProxy.SendMessage(message);
    }

    // Legacy CreateAttacmentsAndSendFeedBackMessage — on publish: stamp IssuingDate, raise the certificate-issued
    // event, then either hand the certificate to the issue-by-worker queue (when IssueCertificateOfOriginByWorker is
    // on) or generate the certificate template inline and save it as an attachment (PrintCertificateOfOriginAndSaveAttachments).
    private async Task PublishAttachments(CertificateOfOrigin entity, IEventUtil eventUtil, int userId)
    {
        entity.IssuingDate = DateTime.Now;
        var issueByWorker = await parametersUtil.Get<bool>("IssueCertificateOfOriginByWorker");
        if (issueByWorker)
        {
            entity.IsInPublishingProcess = true;
        }

        // Persist the issuing date + issue-by-worker flag (legacy PrintCertificateOfOriginAndSaveAttachments Save'd the
        // certificate here) — the main upsert already ran before publish, so these post-publish stamps need their own write.
        await DataLayer.UpdateCertificatePublishingState(entity.Id, entity.IssuingDate.Value, entity.IsInPublishingProcess, userId);

        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateIssued, entity.Id, entity.OrganizationUnitId, null);

        // Issue-by-worker: publish the certificate to the RabbitMQ issue queue instead of generating the template inline.
        if (issueByWorker && entity.IsInPublishingProcess)
        {
            await SendCertificateToIssueQueue(entity);
            return;
        }

        // Inline: generate the certificate template and save it as an attachment (legacy PrintCertificateOfOriginAndSaveAttachments).
        await PrintCertificateOfOriginAndSaveAttachments(entity, string.Empty);
    }

    // Legacy PrintCertificateOfOriginAndSaveAttachments: render the certificate document via SSRS (the Common service's
    // GenerateTemplate) and save it as an attachment on the certificate (GenerateTemplate → SaveCertificateOfOriginAttachments).
    // Shared by the publish flow (#33) and the reconciliation re-print (#34). Rendering is always delegated to SSRS
    // (developer decision — no per-type template switch / no local template files).
    private async Task PrintCertificateOfOriginAndSaveAttachments(CertificateOfOrigin certificate, string additionalInfo)
    {
        var template = await commonServicesProxy.GenerateTemplate(certificate.TypeId, certificate.Id, additionalInfo);
        if (template is null)
        {
            return;
        }

        var args = new SaveCertificateAttachmentsArgsDto
        {
            CertificatesTemplates = [template],
            CertificateId = certificate.Id,
            CertificateNumber = certificate.CertificateNumber,
            CertificateTypeId = certificate.TypeId,
            CertificateRequestReasonCode = certificate.RequestReasonCode,
            AdditionalInfo = additionalInfo,
        };
        await SaveCertificateOfOriginAttachments(args);
    }

    // Legacy SendCertificateToIssueQueue — publish the certificate to the "IssueCertificateOfOrigin" RabbitMQ exchange
    // for asynchronous issuing by a worker (IQueueUtil, mirroring QueueUtilFactory 1:1).
    private async Task SendCertificateToIssueQueue(CertificateOfOrigin entity)
    {
        var payload = new IssueCertificateDto
        {
            CertificateOfOriginId = entity.Id,
            CertificateNumber = entity.CertificateNumber,
            CertificateOfOriginStatusId = entity.CertificateOfOriginStatusId,
            CertificateTypeId = entity.TypeId,
            CertificateTypeName = GetCertificateTypeName(entity.TypeId),
            RequestReasonCode = entity.RequestReasonCode,
            IsInPublishingProcess = entity.IsInPublishingProcess,
            CreateCustomerId = entity.CreateCustomerId,
            RejectCancelReason = entity.RejectCancelReason,
            InternalApplication = entity.InternalApplication,
            FeedbackRemark = entity.FeedbackRemark,
            IssuingDate = entity.IssuingDate,
            Guid = entity.Guid,
            OrganizationUnitId = entity.OrganizationUnitId,
        };

        var queueUtil = Resolve<IQueueUtil>();
        var message = queueUtil.CreateQueueMessageBuilder()
            .SendToExchange(CertificateOfOriginsConsts.IssueCertificateOfOriginExchange)
            .AddCloudEventMessage(payload)
            .Build();
        await queueUtil.SendMessage(message);
    }

    // Legacy HandleCertificateReplacement — cancel the replaced import certificate + raise the replaced event.
    // TODO(migration): the agent talk-back message (SendMessageToAgent) + the replaced-certificate lookup are deferred.
    private async Task HandleCertificateReplacement(CertificateOfOrigin entity, IEventUtil eventUtil)
    {
        if (entity.CertificateIdToCancel is null)
        {
            return;
        }

        await DataLayer.CancelPreviousCertificate(entity.CertificateIdToCancel.Value, string.Empty, RequestMetadata.UserId ?? 0);
        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateReplaced, entity.CertificateIdToCancel.Value, entity.OrganizationUnitId, entity.CertificateNumber);
    }

    // A certificate-scoped event (VirtualEntity(certificate)) with an optional AdditionalInfo.
    private static async Task RaiseCertificateEvent(IEventUtil eventUtil, int eventTypeId, int certificateId, int organizationUnitId, string? additionalInfo)
    {
        var builder = eventUtil.CreatBuilder()
            .WithEventType(eventTypeId)
            .WithEntityId(certificateId)
            .WithEntityType((int)EEntityType.CertificateOfOrigin)
            .WithTitle(certificateId.ToString())
            .WithOrganizationUnitId(organizationUnitId);
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            builder = builder.WithAdditionalInfo(additionalInfo);
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Internal WCF: UpdateCetrificateOfOrigins(dto) — the export-declaration → certificate reconciliation (a one-way
    // DealFile event; the ExportDeclarationSubmissionSucceeded case). For each certificate to reconcile: backfill the
    // declaration link; skip the gate for a non-Received / empty / status-query / cancellation request or a
    // non-manipulation type; otherwise validate it against the declaration (scalar details + destination/origin
    // country-groups + import-replacement trade-agreement + the full invoice / goods-item / customs-item 6-digit
    // matching, both directions), then — unless it is a Draft — set DeclarationMatch / Rejected / DeclarationMismatch,
    // raise the matching event (warnings assign the mismatch task to the assessor), re-print the draft, and append the
    // error rows. Returns the reconciliation errors (the legacy contract is one-way/void; surfacing them is a developer
    // decision).
    // TODO(migration): only the exception text + EMessages code + Error/Warning level source (ValidationMessages/resx,
    // legacy GetUIMessageWithEnglishAndLevel) is still deferred — the checks themselves are migrated.
    public async Task<List<CertificateOfOriginExceptionDto>> UpdateCertificateOfOrigins(UpdateCertificateOfOriginsRequestDto request)
    {
        var exceptions = new List<CertificateOfOriginExceptionDto>();
        if (request.CertificateOfOriginsIds.Count == 0)
        {
            return exceptions;
        }

        var userId = RequestMetadata.UserId ?? 0;
        var eventUtil = Resolve<IEventUtil>();
        var certificates = await DataLayer.GetCertificatesByIds(request.CertificateOfOriginsIds);

        // The certificates' detail rows (destination / origin country, exporter, country groups), loaded once and keyed
        // by certificate id — the reconciliation validator compares them against the declaration.
        var detailsByCertificate = (await DataLayer.GetCertificateDetailsByCertificateIds(certificates.Select(certificate => certificate.Id).ToList()))
            .GroupBy(detail => detail.CertificateOfOriginId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var certificate in certificates)
        {
            // Backfill the declaration link from the DTO when the certificate has none (legacy 492-500).
            var exportDeclarationNumber = string.IsNullOrEmpty(certificate.ExportDeclarationNumber) ? request.ExportDeclarationNum : certificate.ExportDeclarationNumber;
            var leadDocumentId = certificate.LeadDocumentId ?? request.LeadDocumentId;

            // Legacy gate: reconcile only a still-Received certificate that is not an empty / status-query / cancellation
            // request and not a non-manipulation type. Others are only backfilled.
            if (certificate.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Received
                || certificate.RequestReasonCode is (int)ERequestReason.EmptyCertificate or (int)ERequestReason.GetRequestStatus or (int)ERequestReason.CertificateCancellation
                || certificate.TypeId == (int)ECertificateOfOriginType.NonManipulation)
            {
                await DataLayer.UpdateCertificateReconciliation(certificate.Id, certificate.CertificateOfOriginStatusId, exportDeclarationNumber, leadDocumentId, certificate.RejectCancelReason, userId);
                continue;
            }

            var details = detailsByCertificate.GetValueOrDefault(certificate.Id) ?? [];
            var reconciliation = await ValidateReconciliation(request, certificate, details);
            var certificateExceptions = reconciliation.Exceptions;
            exceptions.AddRange(certificateExceptions);

            // A Draft request is reconciled for its error rows but keeps its status and raises no status event (legacy
            // guarded the status change + event with `reason != Draft`).
            var newStatus = certificate.CertificateOfOriginStatusId;
            var rejectReason = certificate.RejectCancelReason;
            if (certificate.RequestReasonCode != (int)ERequestReason.Draft)
            {
                (newStatus, rejectReason) = await ApplyReconciliationOutcome(certificate, reconciliation, request, eventUtil);
            }

            // Re-print the certificate draft template and save it as an attachment (legacy
            // PrintCertificateOfOriginAndSaveAttachments(item, IsDraft)).
            await PrintCertificateOfOriginAndSaveAttachments(certificate, CertificateOfOriginsConsts.IsDraftSentinel);

            await DataLayer.UpdateCertificateReconciliation(certificate.Id, newStatus, exportDeclarationNumber, leadDocumentId, rejectReason, userId);

            if (certificateExceptions.Count > 0)
            {
                // Append the mismatch-log rows (legacy item.CertificateOfOriginVsDeclarationError.Add per exception).
                await DataLayer.AddCertificateVsDeclarationErrors(certificate.Id, certificateExceptions.Select(exception => exception.ExceptionDescription ?? string.Empty).ToList());
            }
        }

        return exceptions;
    }

    // Legacy (non-Draft branch): set the matched / rejected / mismatch status and raise the matching event — warnings
    // additionally raise the import-replacement task and assign the mismatch task to the export-declaration assessor.
    // Returns the new status + reject reason.
    private async Task<(int NewStatus, string? RejectReason)> ApplyReconciliationOutcome(
        CertificateOfOrigin certificate,
        ReconciliationResult reconciliation,
        UpdateCertificateOfOriginsRequestDto request,
        IEventUtil eventUtil)
    {
        var hasErrors = reconciliation.Exceptions.Exists(exception => !exception.ExceptionLevel.HasValue || exception.ExceptionLevel == (int)EExceptionLevel.Error);
        var hasWarnings = reconciliation.Exceptions.Exists(exception => exception.ExceptionLevel == (int)EExceptionLevel.Warning);

        if (!hasErrors && !hasWarnings)
        {
            await RaiseCertificatePreferredAssessorEvent(certificate, eventUtil, (int)EEventType.CertificateOfOriginCertificateMatchDeclaration);
            return ((int)ECertificateOfOriginStatus.DeclarationMatch, certificate.RejectCancelReason);
        }

        // The concatenated exception texts carried as the event AdditionalInfo (legacy, capped).
        var additionalInfo = BuildReconciliationAdditionalInfo(reconciliation.Exceptions);

        if (hasErrors)
        {
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateDeclarationMismatch, certificate.Id, certificate.OrganizationUnitId, additionalInfo);
            return ((int)ECertificateOfOriginStatus.Rejected, CertificateOfOriginsConsts.ReconciliationMismatchReason);
        }

        // Warnings only.
        if (reconciliation.IsLinkedToImportDeclaration)
        {
            // Legacy: an import-certificate replacement whose associated goods are not in the trade agreement opens a
            // task to handle the replacement.
            await RaiseCertificateEvent(eventUtil, (int)EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate, certificate.Id, certificate.OrganizationUnitId, null);
        }

        await RaiseDeclarationHasWarningsEvent(certificate, request, eventUtil, additionalInfo);
        return ((int)ECertificateOfOriginStatus.DeclarationMismatch, certificate.RejectCancelReason);
    }

    // Resolve the export-declaration assessor handling the lead document (legacy
    // GetLatestUserHandlingEntityTasksWithTaskUnification → the resolved user id), or null when there is no lead
    // document / no handling user.
    private async Task<int?> ResolveAssessorUserId(int? leadDocumentId, int organizationUnitId)
    {
        if (!leadDocumentId.HasValue)
        {
            return null;
        }

        var filter = new LatestUserHandlingEntityTasksFilterDto
        {
            EntityId = leadDocumentId.Value,
            EntityTypeId = CertificateOfOriginsConsts.ExportLeadDocumentEntityType,
            OrganizationUnitTypeId = CertificateOfOriginsConsts.ExportOrganizationUnitType,
            OrganizationUnitId = organizationUnitId,
        };
        return await tasksProxy.GetLatestUserHandlingEntityTasksWithTaskUnification(filter);
    }

    // Legacy RaiseTaskNewCertificateOfOriginCheck: raise the given certificate event, preferring the export-declaration
    // assessor for the opened task (legacy EventTaskArguments.PreferredUserID; the assessor OrganizationUnitId source is
    // the certificate's). Used by the reconciler match event and the new-certificate-created event.
    // TODO(migration): the "additional certificates on this declaration" task message
    // (EMessages.AdditionalCertificatesToExportDeclaration + GetCertificateOfOriginByDeclaration) is deferred (resx).
    private async Task RaiseCertificatePreferredAssessorEvent(CertificateOfOrigin certificate, IEventUtil eventUtil, int eventTypeId)
    {
        var assessorUserId = await ResolveAssessorUserId(certificate.LeadDocumentId, certificate.OrganizationUnitId);
        var builder = eventUtil.CreatBuilder()
            .WithEventType(eventTypeId)
            .WithEntityId(certificate.Id)
            .WithEntityType((int)EEntityType.CertificateOfOrigin)
            .WithTitle(certificate.Id.ToString())
            .WithOrganizationUnitId(certificate.OrganizationUnitId);
        if (assessorUserId.HasValue)
        {
            builder = builder.WithTaskArguments(task => task.WithPreferredUserId(assessorUserId.Value));
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy warnings branch: raise CertificateOfOriginCertificateDeclarationHasWarnings, assigning the mismatch task to
    // the export-declaration assessor (the resolved user id; the assessor OrganizationUnitId source here is the DTO's,
    // per the legacy). The task is assigned only when an assessor is found.
    private async Task RaiseDeclarationHasWarningsEvent(CertificateOfOrigin certificate, UpdateCertificateOfOriginsRequestDto request, IEventUtil eventUtil, string additionalInfo)
    {
        var assessorUserId = await ResolveAssessorUserId(certificate.LeadDocumentId, request.OrganizationUnitId);
        var builder = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.CertificateOfOriginCertificateDeclarationHasWarnings)
            .WithEntityId(certificate.Id)
            .WithEntityType((int)EEntityType.CertificateOfOrigin)
            .WithTitle(certificate.Id.ToString())
            .WithOrganizationUnitId(certificate.OrganizationUnitId);
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            builder = builder.WithAdditionalInfo(additionalInfo);
        }

        if (assessorUserId.HasValue)
        {
            // TODO(migration): the legacy SingleUserAssignmentFilter also carried EProfession.Marech + the Export
            // organization-unit-type; the new builder assigns by the resolved assessor user id only.
            builder = builder.WithTaskArguments(task => task.WithTaskAssignmentUser(assessorUserId.Value));
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy: concatenate the reconciliation exception texts into the event AdditionalInfo, capped so the task field is
    // not overflowed (MaximumNumberOfCharactersOfTheField, reserving LengthOfTaskStart).
    private static string BuildReconciliationAdditionalInfo(IEnumerable<CertificateOfOriginExceptionDto> exceptions)
    {
        var additionalInfo = string.Empty;
        foreach (var exception in exceptions)
        {
            var text = exception.ExceptionDescription ?? string.Empty;
            if (additionalInfo.Length + text.Length + CertificateOfOriginsConsts.LengthOfTaskStart < CertificateOfOriginsConsts.MaximumNumberOfCharactersOfTheField)
            {
                additionalInfo += text + " , ";
            }
        }

        return additionalInfo;
    }

    // Legacy ValidateExportDeclarationInfoForPCIsMatch: compares the certificate against the export declaration and
    // collects the mismatch exceptions (deduped by message, like legacy PassGroupdExceptionListToEntity's GroupBy).
    // Levels follow the legacy (a null level counts as an error; only the import-replacement and unmatched-invoice
    // checks are warnings). Returns the exceptions + whether the certificate is now linked to an import declaration.
    // TODO(migration): the localized + English exception texts and their EMessages codes are placeholders here — source
    // them from ValidationMessages/resx (legacy SystemTablesUtil.GetUIMessageWithEnglishAndLevel gives text+english+level).
    private async Task<ReconciliationResult> ValidateReconciliation(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateOfOriginDetails> details)
    {
        var builder = new List<ReconciliationException>();

        // No invoices in the declaration → the certificate cannot be reconciled (legacy: hasErrors = true).
        if (request.ExportInvoiceInfoList.Count == 0)
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.NoExportInvoices,
                (int)EExceptionLevel.Error,
                "אין חשבוניות בהצהרת היצוא",
                "No export invoices in the declaration."));
            return BuildReconciliationResult(builder, false);
        }

        await ValidateCertificateDetails(request, certificate, details, builder);
        var isLinkedToImportDeclaration = await ValidateImportReplacement(request, certificate, builder);

        // Invoice / goods-item / customs-item matching (the declaration already carries invoices — checked above).
        var invoices = await DataLayer.GetCertificateInvoiceDetailsByCertificateIds([certificate.Id]);
        if (invoices.Count > 0)
        {
            var originCountry = GetDetailValue(details, ECertificateDetailsType.OriginCountry);
            var originGroup = GetDetailValue(details, ECertificateDetailsType.OriginGroupOfCountries);
            await ValidateInvoiceMatching(request, certificate, invoices, originCountry, originGroup, builder);
        }

        return BuildReconciliationResult(builder, isLinkedToImportDeclaration);
    }

    // Scalar/detail checks: destination country + destination country-group, export-declaration number, exporter
    // (all Error level, since the legacy raised them with no explicit level).
    private async Task ValidateCertificateDetails(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateOfOriginDetails> details,
        List<ReconciliationException> builder)
    {
        var destinationCountry = GetDetailValue(details, ECertificateDetailsType.DestinationCountry);
        var destinationGroup = GetDetailValue(details, ECertificateDetailsType.DestinationGroupOfCountries);
        var exporterId = GetDetailValue(details, ECertificateDetailsType.ExporterId);

        // Destination country on the certificate must match the declaration's destination (Error).
        if (request.DestinationCountryId.HasValue
            && int.TryParse(destinationCountry, out var destinationCountryId)
            && destinationCountryId != request.DestinationCountryId.Value)
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.DestinationCountryMismatch,
                (int)EExceptionLevel.Error,
                "ארץ היעד בתעודה אינה תואמת לארץ היעד בהצהרת היצוא",
                "The destination country does not match the destination country in the export declaration."));
        }

        // Destination country-group agreement — the buyer country must belong to the certificate's destination group
        // (Error). Legacy looks up the (destination country, group) pair; a null declaration destination country cannot
        // match the pair, so it is a discrepancy — the check is NOT skipped on a null destination country.
        if (!string.IsNullOrWhiteSpace(destinationGroup) && int.TryParse(destinationGroup, out var destinationGroupId))
        {
            var destinationInGroup = request.DestinationCountryId.HasValue
                && await countryGroupProxy.IsCountryInCountryGroup(request.DestinationCountryId.Value, destinationGroupId);
            if (!destinationInGroup)
            {
                builder.Add(new ReconciliationException(
                    EReconciliationMessage.DestinationGroupDiscrepancy,
                    (int)EExceptionLevel.Error,
                    "אי התאמה בין הארצות בהסכם לבין ארץ הקונה",
                    "Discrepancy between the countries in the agreement and the buyer country."));
            }
        }

        // The export-declaration number already stamped on the certificate must match the declaration's (legacy
        // ExportDeclarationNotInSystemForWarningMessage — raised with no explicit level, so null / Error per legacy).
        if (!string.IsNullOrEmpty(certificate.ExportDeclarationNumber)
            && certificate.ExportDeclarationNumber != request.ExportDeclarationNum)
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.ExportDeclarationNotInSystem,
                (int)EExceptionLevel.Error,
                $"מספר הצהרת היצוא {request.ExportDeclarationNum} אינו קיים במערכת",
                $"Export declaration {request.ExportDeclarationNum} is not in the system."));
        }

        // The exporter on the certificate must match the declaration's exporter (Error; the legacy compares int to
        // nullable int, so a null declaration exporter also flags a mismatch — preserved).
        if (int.TryParse(exporterId, out var exporter) && exporter != request.ExporterCustomerId)
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.ExporterMismatch,
                (int)EExceptionLevel.Error,
                "מספר היצואן בתעודה אינו תואם למספר היצואן בהצהרת היצוא",
                "The exporter number does not match the exporter number in the export declaration."));
        }
    }

    // Import-certificate replacement: at least one associated import goods item's origin country must be party to the
    // certificate type's trade agreement, else a warning + the certificate is linked to an import declaration (returns
    // isLinkedToImportDeclaration).
    private async Task<bool> ValidateImportReplacement(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<ReconciliationException> builder)
    {
        if (certificate.RequestReasonCode != (int)ERequestReason.ImportCertificateReplacement)
        {
            return false;
        }

        var associatedGoodsItems = await exportDealFileProxy.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(request.LeadDocumentId);
        var isInTradeAgreement = false;
        foreach (var goodsItem in associatedGoodsItems ?? [])
        {
            if (await customsBookProxy.IsTradeAgreementForCountry(certificate.TypeId, goodsItem.AssociatedOriginCountryId, false))
            {
                isInTradeAgreement = true;
                break;
            }
        }

        if (associatedGoodsItems is { Count: > 0 } && isInTradeAgreement)
        {
            return false;
        }

        builder.Add(new ReconciliationException(
            EReconciliationMessage.ImportReplacementNoTradeAgreement,
            (int)EExceptionLevel.Warning,
            "אין בהצהרה טובין המקושרים להצהרת היבוא שארץ המקור שלהם נמצאת בהסכם הסחר",
            "There is no merchandise in the declaration linked to the import declaration whose country of origin is in the trade agreement."));
        return true;
    }

    // Legacy ValidateExportDeclarationInfoForPCIsMatch invoice block (item.CertificateOfOriginInvoiceDetail loop): each
    // certificate invoice must match a declaration invoice by number, and — when the certificate type requires it — the
    // goods items' origin country / country-group / certificate link / 6-digit customs classification must match, in
    // both directions.
    private async Task ValidateInvoiceMatching(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        string? originCountry,
        string? originGroup,
        List<ReconciliationException> builder)
    {
        var isCustomsItemMandatory = await DataLayer.GetCertificateTypeIsCustomsItemMandatory(certificate.TypeId) ?? false;

        // Resolve the 6-digit tariff classification of every customs item on both sides in one batch.
        var certificateCustomsItemIds = invoices.SelectMany(invoice => invoice.CustomsItemIds);
        var declarationCustomsItemIds = request.ExportInvoiceInfoList
            .SelectMany(invoice => invoice.ExportGoodsItemInfoList)
            .Select(goodsItem => goodsItem.CustomsItemId);
        var filters = certificateCustomsItemIds
            .Concat(declarationCustomsItemIds)
            .Distinct()
            .Select(customsItemId => new CustomsItemsIdsCacheFilterDto { CustomsItemId = customsItemId, Date = certificate.CreateDate })
            .ToList();
        var customsItems = await customsBookProxy.GetCustomsItemsByIds(filters) ?? [];
        var sixDigitByCustomsItemId = customsItems
            .GroupBy(customsItem => customsItem.Id)
            .ToDictionary(group => group.Key, group => SixDigits(group.First().FullClassification));

        // Forward: every certificate invoice + goods item against the declaration.
        var allInvoicesMatched = await ValidateCertificateInvoices(request, certificate, invoices, originCountry, originGroup, sixDigitByCustomsItemId, isCustomsItemMandatory, builder);

        // Reverse: every declaration goods item linked to this certificate must have its 6-digit classification present
        // in the certificate's matching invoice (Error) — only when the type requires it and all invoices matched.
        if (allInvoicesMatched && isCustomsItemMandatory)
        {
            ValidateDeclarationGoodsItems(request, certificate, invoices, sixDigitByCustomsItemId, builder);
        }
    }

    // Forward direction: each certificate invoice must match a declaration invoice by number (else a warning + stop),
    // then each of its goods items is validated. Returns false if an invoice did not match (legacy returned from the
    // whole validator on the first unmatched invoice).
    private async Task<bool> ValidateCertificateInvoices(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        string? originCountry,
        string? originGroup,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        bool isCustomsItemMandatory,
        List<ReconciliationException> builder)
    {
        foreach (var invoice in invoices)
        {
            var declarationInvoice = request.ExportInvoiceInfoList.FirstOrDefault(declaration => declaration.ExternalIdNum == invoice.InvoiceNumber);
            if (declarationInvoice == null)
            {
                builder.Add(new ReconciliationException(
                    EReconciliationMessage.ExportInvoiceNotMatch,
                    (int)EExceptionLevel.Warning,
                    $"חשבונית היצוא {invoice.InvoiceNumber} אינה תואמת לחשבונית בהצהרת היצוא",
                    $"Export invoice {invoice.InvoiceNumber} does not match an invoice in the export declaration."));
                return false;
            }

            // Legacy runs the per-goods-item checks only when BOTH sides are non-empty; the certificate side is implicit
            // (the loop below is empty when it has no customs items). Skip when the matched declaration invoice carries
            // no goods items — otherwise the absence-based checks (!Any) would false-positive on the empty list.
            if (declarationInvoice.ExportGoodsItemInfoList.Count == 0)
            {
                continue;
            }

            var declarationCustomsItemIdsForInvoice = declarationInvoice.ExportGoodsItemInfoList
                .Where(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id)
                .Select(goodsItem => goodsItem.CustomsItemId)
                .ToList();

            foreach (var certificateCustomsItemId in invoice.CustomsItemIds)
            {
                await ValidateCertificateGoodsItem(certificate, declarationInvoice, invoice.InvoiceNumber, certificateCustomsItemId, declarationCustomsItemIdsForInvoice, originCountry, originGroup, sixDigitByCustomsItemId, isCustomsItemMandatory, builder);
            }
        }

        return true;
    }

    // One certificate goods item vs the matched declaration invoice: origin country, origin country-group, certificate
    // link, and the forward customs-item 6-digit match.
    private async Task ValidateCertificateGoodsItem(
        CertificateOfOrigin certificate,
        ExportInvoiceInfoDto declarationInvoice,
        string? invoiceNumber,
        int certificateCustomsItemId,
        List<int> declarationCustomsItemIdsForInvoice,
        string? originCountry,
        string? originGroup,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        bool isCustomsItemMandatory,
        List<ReconciliationException> builder)
    {
        // Origin country present among the declaration's goods items (Error).
        if (int.TryParse(originCountry, out var originCountryId)
            && !declarationInvoice.ExportGoodsItemInfoList.Any(goodsItem => goodsItem.OriginCountryId == originCountryId))
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.OriginCountryMismatch,
                (int)EExceptionLevel.Error,
                "ארץ המקור בתעודה אינה תואמת לארץ המקור בהצהרת היצוא",
                "The origin country does not match the origin country in the export declaration."));
        }

        // Origin country-group agreement — any declaration origin country must be in the group (Error).
        if (!string.IsNullOrWhiteSpace(originGroup) && int.TryParse(originGroup, out var originGroupId))
        {
            var anyOriginInGroup = false;
            foreach (var goodsItem in declarationInvoice.ExportGoodsItemInfoList)
            {
                if (await countryGroupProxy.IsCountryInCountryGroup(goodsItem.OriginCountryId, originGroupId))
                {
                    anyOriginInGroup = true;
                    break;
                }
            }

            if (!anyOriginInGroup)
            {
                builder.Add(new ReconciliationException(
                    EReconciliationMessage.OriginCountryMismatch,
                    (int)EExceptionLevel.Error,
                    "ארץ המקור בתעודה אינה תואמת לארץ המקור בהצהרת היצוא",
                    "The origin country does not match the origin country in the export declaration."));
            }
        }

        // The certificate must be linked to at least one declaration goods item (Error).
        if (!declarationInvoice.ExportGoodsItemInfoList.Any(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id))
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.CertificateNumberNotInDealFile,
                (int)EExceptionLevel.Error,
                "מספר התעודה אינו תואם למספר התעודה בקובץ עסקת היצוא",
                "The certificate number does not match the certificate number in the export deal file."));
        }

        // Customs-item 6-digit classification match, forward (Error) — only when the type requires it.
        if (isCustomsItemMandatory
            && sixDigitByCustomsItemId.TryGetValue(certificateCustomsItemId, out var certificateSixDigits)
            && !string.IsNullOrEmpty(certificateSixDigits)
            && !declarationCustomsItemIdsForInvoice.Any(declarationCustomsItemId =>
                sixDigitByCustomsItemId.TryGetValue(declarationCustomsItemId, out var declarationSixDigits)
                && !string.IsNullOrEmpty(declarationSixDigits)
                && declarationSixDigits == certificateSixDigits))
        {
            builder.Add(new ReconciliationException(
                EReconciliationMessage.CustomsItemMismatch,
                (int)EExceptionLevel.Error,
                $"פריט המכס {certificateSixDigits} בחשבונית {invoiceNumber} אינו תואם לפריט מכס בהצהרת היצוא",
                $"Customs item {certificateSixDigits} in invoice {invoiceNumber} does not match a customs item in the export declaration."));
        }
    }

    // Reverse direction: every declaration goods item linked to this certificate must have its 6-digit classification
    // present in the certificate's matching invoice (Error).
    private static void ValidateDeclarationGoodsItems(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        List<ReconciliationException> builder)
    {
        foreach (var declarationInvoice in request.ExportInvoiceInfoList)
        {
            var certificateCustomsItemIdsForInvoice = invoices
                .FirstOrDefault(invoice => invoice.InvoiceNumber == declarationInvoice.ExternalIdNum)?.CustomsItemIds ?? [];
            foreach (var goodsItem in declarationInvoice.ExportGoodsItemInfoList.Where(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id))
            {
                if (sixDigitByCustomsItemId.TryGetValue(goodsItem.CustomsItemId, out var declarationSixDigits)
                    && !string.IsNullOrEmpty(declarationSixDigits)
                    && !certificateCustomsItemIdsForInvoice.Any(certificateCustomsItemId =>
                        sixDigitByCustomsItemId.TryGetValue(certificateCustomsItemId, out var certificateSixDigits)
                        && !string.IsNullOrEmpty(certificateSixDigits)
                        && certificateSixDigits == declarationSixDigits))
                {
                    builder.Add(new ReconciliationException(
                        EReconciliationMessage.CustomsItemInDeclarationNotInCertificate,
                        (int)EExceptionLevel.Error,
                        $"פריט המכס {declarationSixDigits} בחשבונית {declarationInvoice.ExternalIdNum} אינו תואם לפריטי המכס בתעודה",
                        $"Customs item {declarationSixDigits} in invoice {declarationInvoice.ExternalIdNum} does not match the customs items in the certificate."));
                }
            }
        }
    }

    private static string? GetDetailValue(List<CertificateOfOriginDetails> details, ECertificateDetailsType detailType)
    {
        return details.FirstOrDefault(detail => detail.CertificateDetailsTypeCodeId == (int)detailType)?.Value;
    }

    private static string? SixDigits(string? fullClassification)
    {
        return !string.IsNullOrEmpty(fullClassification) && fullClassification.Length >= 6
            ? fullClassification[..6]
            : null;
    }

    // Legacy PassGroupdExceptionListToEntity: dedup by message (GroupBy UserMessage, first wins), then project to DTOs.
    private static ReconciliationResult BuildReconciliationResult(List<ReconciliationException> builder, bool isLinkedToImportDeclaration)
    {
        var exceptions = builder
            .GroupBy(exception => exception.Key)
            .Select(group => group.First())
            .Select(exception => new CertificateOfOriginExceptionDto
            {
                ExceptionLevel = exception.Level,
                ExceptionDescription = exception.Description,
                EnglishDescription = exception.EnglishDescription,
                ExceptionType = 0, // TODO(migration): the real EMessages code (legacy GetUIMessageWithEnglishAndLevel).
            })
            .ToList();
        return new ReconciliationResult(exceptions, isLinkedToImportDeclaration);
    }

    // Dedup key for the reconciliation exceptions — reproduces the legacy GroupBy(InfException.UserMessage). Each member
    // corresponds to a legacy EMessages code (noted below); ExceptionType is left 0 for now because those codes + their
    // text live in the SystemTables message table, whose .NET 10 equivalent (ValidationMessages/resx) is blocked on the
    // BaseValidationMessages package (see Program.cs). TODO(migration): when it lands, set ExceptionType to the real
    // EMessages code and source the text from ValidationMessages.
    private enum EReconciliationMessage
    {
        NoExportInvoices,                           // migration-added (legacy set hasErrors without an EMessages)
        DestinationCountryMismatch,                 // EMessages.DestinationCountryIsNotMAtchTofDestinationCountryInExportdeclaration
        DestinationGroupDiscrepancy,                // EMessages.DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer
        ExportDeclarationNotInSystem,               // EMessages.ExportDeclarationNotInSystemForWarningMessage
        ExporterMismatch,                           // EMessages.ExporterNumberIsNotMAtchToExporterNumberInExportdeclaration
        ImportReplacementNoTradeAgreement,          // EMessages.ThereIsNoMerchandiseInTheDeclarationThatIsLinkedToTheImportDeclarationWhoseCountryOfOriginIsInTheTradeAgreement
        OriginCountryMismatch,                      // EMessages.OriginCountryIsNotMAtchToOriginCountryInExportdeclaration
        CertificateNumberNotInDealFile,             // EMessages.CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile
        CustomsItemMismatch,                        // EMessages.CustomsItemIsNotMAtchToCustomsItemInExportdeclaration
        ExportInvoiceNotMatch,                      // EMessages.ExportInvoiceIsNotMAtchToExportInvoiceInExportdeclaration
        CustomsItemInDeclarationNotInCertificate,   // EMessages.CustomsItemInDeclarationIsNotMAtchToCustomsItemsInCertificate
    }

    private sealed class ReconciliationException(EReconciliationMessage key, int level, string description, string englishDescription)
    {
        public EReconciliationMessage Key { get; } = key;

        public int Level { get; } = level;

        public string Description { get; } = description;

        public string EnglishDescription { get; } = englishDescription;
    }

    private sealed class ReconciliationResult(List<CertificateOfOriginExceptionDto> exceptions, bool isLinkedToImportDeclaration)
    {
        public List<CertificateOfOriginExceptionDto> Exceptions { get; } = exceptions;

        public bool IsLinkedToImportDeclaration { get; } = isLinkedToImportDeclaration;
    }
}
