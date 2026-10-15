using CertificateOfOrigins.BL.Validations;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;
using CustomsCloud.InfrastructureCore.Lock;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Parameters;
using CustomsCloud.InfrastructureCore.Queue;
using CustomsCloud.InfrastructureCore.Utils.Documents;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace CertificateOfOrigins.BL;

public partial class CertificateOfOriginsBl(IServiceProvider serviceProvider, ILookupUtil lookupUtil, IParametersUtil parametersUtil)
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

    // Decided (parity A-24, 2026-10-08): every milestone row is kept, with an empty name for a user the Users service does
    // not know - legacy's INNER JOIN on UserMng_User dropped such rows. The name is the user's Title, as legacy read it (the
    // Users service contract is not published yet: UserDto.Title carries a TODO(blocking)).
    private async Task FillMilestoneUserNames(CertificateOfOriginDto certificate)
    {
        var userProxy = Resolve<IUserProxy>();
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
                milestone.UserName = user.Title;
            }
        }
    }

    #region GetPC_MSG2280_2281 (Incoming / EAI certificate-of-origin request)

    // Incoming/EAI WCF: GetPC_MSG2280_2281_CertificateOfOriginRequest (PC_NG_2280 → feedback PC_NG_2281). Processes an
    // agent's certificate-of-origin request and returns the feedback synchronously — the legacy one-way callback/MSMQ
    // response is exposed here as a direct REST return (developer decision: mirrors the legacy *Sync contract and the
    // migrated sibling GetCertificateRequestByGuid).
    //
    // Branches by request reason: GetRequestStatus and CertificateCancellation read/cancel the saved certificate; every
    // other reason is the create/update branch (ProcessCreateCertificateBranch: message validation, which also resolves
    // the exporter / destination-country / org-unit / cert-to-update values → SaveCertificateOfOrigin → declaration
    // check and reconciliation).
    //
    // Legacy generated wrapper (CertificateOfOriginsIncomingMessageService.GetPC_MSG2280_2281_CertificateOfOriginRequest):
    // EVERYTHING the message does - reading the config, taking the lock, processing, a failed external service - runs
    // inside one try; every exception is caught and returned to the sender in the response header
    // (HandleMessageException), never as a fault. The message channel expects a response.
#pragma warning disable CA1031 // catch-all on purpose: the legacy contract answers every message in-band
    public async Task<CertificateOfOriginRequestFeedbackResponseDto> GetPC22802281CertificateOfOriginRequest(CertificateOfOriginRequestMessageDto request)
    {
        try
        {
            var result = await GetPC22802281CertificateOfOriginRequestUnderLock(request);
            return result;
        }
        catch (Exception exception)
        {
            Resolve<ILogger<CertificateOfOriginsBl>>().LogError(exception, "GetPC_MSG2280_2281 failed; answered in-band with GeneralException.");
            var failedResponse = BuildInBandErrorResponse(EMessageCode.GeneralException);
            return failedResponse;
        }
    }
#pragma warning restore CA1031

    // Legacy InternalGetPC_MSG2280_2281_CertificateOfOriginRequest.
    private async Task<CertificateOfOriginRequestFeedbackResponseDto> GetPC22802281CertificateOfOriginRequestUnderLock(CertificateOfOriginRequestMessageDto request)
    {
        // Legacy dereferenced request.Content.AgentRequest on its first line, so a message without one failed with a
        // NullReferenceException that the wrapper answered in-band (GeneralException).
        if (request.AgentRequest is null)
        {
            throw new InvalidOperationException("GetPC_MSG2280_2281 received no AgentRequest.");
        }

        // An optional distributed lock (config IsNeedToLockCertificateOfOrigin), keyed by the certificate id, serializes
        // concurrent requests for the same certificate; released in a finally.
        var lockKey = request.AgentRequest.CertificateId;
        var needLock = !string.IsNullOrEmpty(lockKey) && await parametersUtil.Get<bool>("IsNeedToLockCertificateOfOrigin");
        if (!needLock)
        {
            var unlockedResult = await ProcessCertificateOfOriginRequest(request);
            return unlockedResult;
        }

        var lockUtil = Resolve<ILockUtil>();
        ILockState? lockState = null;
        try
        {
            lockState = await lockUtil.LockUntilAsync(lockKey!, TimeSpan.FromMinutes(5), nameof(GetPC22802281CertificateOfOriginRequest));
            if (!lockState.IsAcquired)
            {
                // Legacy threw InfException(ConcurrencyErrorTryAgain) here; the wrapper answered it in-band with that
                // exception in the response header (a 200 the sender can retry on), not as a fault.
                var lockedResponse = BuildInBandErrorResponse(EMessageCode.ConcurrencyErrorTryAgain);
                return lockedResponse;
            }

            var result = await ProcessCertificateOfOriginRequest(request);
            return result;
        }
        finally
        {
            // Legacy also called SafeReleaseAsync with an UNACQUIRED state; only a lock this request took is released here.
            // Since Lock 1.10.54 SafeReleaseAsync releases in the database the lock was taken in, only when the stored
            // LockId still matches this state's, and returns false for an unacquired state.
            if (lockState is { IsAcquired: true })
            {
                await lockUtil.SafeReleaseAsync(lockKey!, lockState);
            }
        }
    }

    // The legacy wrapper's fresh response for a failed message: an empty feedback and the failure in the header exceptions.
    private static CertificateOfOriginRequestFeedbackResponseDto BuildInBandErrorResponse(EMessageCode code)
    {
        var response = new CertificateOfOriginRequestFeedbackResponseDto
        {
            Feedback = new CertificateOfOriginRequestFeedbackDto(),
            Exceptions = [BuildMessageException(code)],
        };
        return response;
    }

    private async Task<CertificateOfOriginRequestFeedbackResponseDto> ProcessCertificateOfOriginRequest(CertificateOfOriginRequestMessageDto request)
    {
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var agentRequest = request.AgentRequest;
        var reasonCode = agentRequest.RequestReasonCode;

        // Legacy CheckRequestReasonAndGetSavedCertificate: the existing certificate the reason refers to. Not-found /
        // missing-id are accumulated as in-band request exceptions (legacy _requestExceptions), NOT thrown — the legacy
        // returns them on the feedback response, it does not 404 (mirrors GetCertificateRequestByGuid's in-band contract).
        var requestExceptions = new List<CertificateOfOriginExceptionDto>();

        // One per-request context spanning the whole message flow — carries the export-declaration-details cache so the
        // amendment guard, the per-reason declaration check and the post-save reconciliation share a single fetch.
        var context = new MessageValidationContext();

        // Legacy CheckRequestReasonCodeEnum (CertificateOfOriginsIncomingMessageServicePartial.cs:1384-1406): the request
        // reason must be present (!= 0) and a known code. Accumulated in-band (not thrown), matching legacy — otherwise an
        // unrecognised/zero reason falls through the switch default straight into the create branch instead of being
        // rejected; the accumulated error blocks the save at the create branch's exception gate.
        if (reasonCode == 0)
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.MandatoryValue, nameof(agentRequest.RequestReasonCode)));
        }
        else if (!Enum.IsDefined(typeof(ERequestReason), reasonCode))
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.RequestReasonNotExist));
        }

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
                // Legacy validated a status query like any other message (type, fields, invoices; no body is required)
                // and any error suppressed the status: the response then carries only the exceptions, with an empty
                // feedback (the legacy throw → fresh response).
                await ValidateMessageBody(request, context, requestExceptions);
                requestExceptions.AddRange(context.Exceptions);
                certificateToResponse = await GetSavedCertificateForMessage(agentRequest, requestExceptions);
                if (requestExceptions.Count > 0)
                {
                    certificateToResponse = null;
                }

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

                AddOpenCustomsEmployeeTaskException(certificateToResponse, requestExceptions);

                // Legacy: the exception gate (throw _requestExceptions) runs BEFORE the reason switch, so the cancel is
                // performed only when NOTHING accumulated — an amendment-linkage error, a not-found, or an associated
                // declaration all block it. Only cancel when the request is clean.
                if (certificateToResponse != null && requestExceptions.Count == 0)
                {
                    await CancelCertificateOfOriginFromMessage(certificateToResponse);
                }

                // The same gate threw, and the legacy wrapper then answered with a FRESH response - empty feedback,
                // ApplicationId 0, only the header exceptions - so a blocked cancellation must not echo the certificate
                // it did not cancel (a consumer reading a non-zero ApplicationId would take it for success).
                if (requestExceptions.Count > 0)
                {
                    certificateToResponse = null;
                }

                break;

            default:
                certificateToResponse = await ProcessCreateCertificateBranch(request, context, requestExceptions);
                break;
        }

        // Legacy: an unresolved certificate leaves the feedback body empty (the response header still carries the
        // accumulated exceptions). The read/cancel branches surface not-found this way rather than as a 404. All
        // exceptions — validation, not-found and declaration reconciliation — flow through the single requestExceptions
        // channel.
        var response = await BuildRequestFeedbackResponse(certificateToResponse, requestExceptions, reasonCode);
        return response;
    }

    // Legacy CheckCertificateNumber → GetCertificateOfOriginByExternalId: the latest certificate with this number.
    // Missing id → MustSendCertificateID; not found → CertificateDoesntExist. Both are accumulated as in-band request
    // exceptions (not thrown), matching the legacy _requestExceptions contract.
    private async Task<CertificateOfOrigin?> GetSavedCertificateForMessage(CertificateOfOriginAgentRequestDto agentRequest, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        if (string.IsNullOrWhiteSpace(agentRequest.CertificateId))
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

        // Legacy EMessages.CertificateOfOriginCancelFromMessage (from the UIMessage table).
        certificate.RejectCancelReason = ErrorMessagesResources.CertificateOfOriginCancelFromMessage;
        await DataLayer.CancelCertificate(certificate.Id, certificate.RejectCancelReason, RequestMetadata.UserId ?? 0);

        var eventUtil = Resolve<IEventUtil>();
        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginUserCancelledCertificate, certificate, certificate.RejectCancelReason);
    }

    // Legacy CreateCertificateOfOriginRequestFeedbackResponse: the feedback DTO + (create-branch) attachments. The
    // reconciliation exceptions (from the post-save declaration check) and the in-band request exceptions (not-found /
    // missing-id, accumulated above) are merged onto the response — the legacy returns them here, it does not throw.
    private async Task<CertificateOfOriginRequestFeedbackResponseDto> BuildRequestFeedbackResponse(CertificateOfOrigin? certificate, List<CertificateOfOriginExceptionDto> requestExceptions, int requestReasonCode)
    {
        // A resolved certificate carries the full feedback; an unresolved one (not-found) leaves the feedback empty and
        // relies on the exceptions to convey the failure — the certificate id is then unknown (0).
        var feedback = certificate != null ? await BuildRequestFeedback(certificate) : new CertificateOfOriginRequestFeedbackDto();
        var response = new CertificateOfOriginRequestFeedbackResponseDto
        {
            ApplicationId = certificate?.Id ?? 0,
            Feedback = feedback,
            Exceptions = requestExceptions.Count > 0 ? requestExceptions : null,
            Attachments = certificate != null ? await BuildFeedbackAttachments(certificate, requestReasonCode) : null,
        };
        return response;
    }

    // Legacy CertificateOfOriginsUtil.CreateAttachments, and the condition that guards it at the response site: no
    // attachment when the request reason is EmptyCertificate, CertificateCancellation or GetRequestStatus, nor when the
    // reason is not Draft and the certificate is not Published - otherwise the rendered attachments.
    //
    // requestReasonCode is the INCOMING message's reason, not the stored certificate's: a status query or a cancellation
    // returns an existing certificate whose own reason (e.g. NewCertificate) differs from the message's.
    //
    // i.e. the agent gets the rendered document back for a Draft request, or once the certificate reaches
    // Published — and never for the three read/cancel reasons. The migration hard-coded null here, so an agent
    // sending a Draft request got no document at all even though one had been rendered and stored. CHECK 2, 2026-09-07.
    //
    // Legacy renders at response-build time (CreateAttachments calls the render itself) rather than reusing an
    // earlier render, and that is reproduced: the save path already re-uploads over the same document, so rendering
    // here does not accumulate duplicates.
    //
    // ⚠️ Legacy could return up to THREE templates — the main one plus eurPage2Template (page 2 of an EUR
    // certificate) and templateForView. The migrated render produces one. That gap is inside the renderer, not
    // here; recorded as a separate finding rather than widened into this fix.
    private async Task<List<CertificateOfOriginMessageAttachmentDto>?> BuildFeedbackAttachments(CertificateOfOrigin certificate, int requestReasonCode)
    {
        if (requestReasonCode is (int)ERequestReason.EmptyCertificate or (int)ERequestReason.CertificateCancellation or (int)ERequestReason.GetRequestStatus
            || (requestReasonCode != (int)ERequestReason.Draft && certificate.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published))
        {
            return null;
        }

        var template = await PrintCertificateOfOriginAndSaveAttachments(certificate, string.Empty);
        if (template is null)
        {
            return null;
        }

        return
        [
            new CertificateOfOriginMessageAttachmentDto
            {
                DocumentTypeId = template.DocumentTypeId,
                Content = template.Content,
                FileName = template.FileName,
            },
        ];
    }

    // Legacy CreateCertificateOfOriginRequestFeedback: echo the certificate identity/status + the public query URL.
    private async Task<CertificateOfOriginRequestFeedbackDto> BuildRequestFeedback(CertificateOfOrigin certificate)
    {
        var urlTemplate = await parametersUtil.Get<string>("CertificateOfOriginQueryURL");
        var queryUrl = string.Format(CultureInfo.InvariantCulture, urlTemplate ?? string.Empty, certificate.Guid?.ToString());

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

            // IssueDateIfReleased / IssueDateIfNotReleased stay null in every branch, as legacy: its
            // CreateCertificateOfOriginRequestFeedback sets them only when IsDeclarationReleased.HasValue, and that
            // unmapped flag is set only by LoadDataFromExportDeclaration (the edit screen), never in the message flow.
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
        var documentsProxy = Resolve<IDocumentsProxy>();
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

            // Legacy: SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode — Lookups.CurrencyType.
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
        var currencies = await lookupUtil.Search<Lookups.CurrencyType>(currency => currencyTypeIds.Contains(currency.Id) && currency.State == CertificateOfOriginsConsts.ActiveState);
        var result = currencies.ToDictionary(currency => currency.Id, currency => currency.CurrencyCode);
        return result;
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

        // Legacy put the DateTime itself in the loosely-typed Value and the serializer wrote it as "yyyy-MM-dd"; the issuing
        // date is stamped with DateTime.Now, so the platform serializer would add a time part (parity finding D F2).
        fieldDataDtos.Add(new FieldDataDto
        {
            Label = CertificateOfOriginsConsts.IssuingDateLabel,
            Value = certificate.IssuingDate?.ToString(WebDateJsonConverter.Format, CultureInfo.InvariantCulture),
        });

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

    // The short-date display of a stored date detail value (legacy Check*Date: DateTime.TryParse, an unparseable value
    // yields default(DateTime)). Parsed day-first like legacy's host culture: legacy-stored values are dd/MM/yyyy (an
    // invariant parse flips day and month, or fails above day 12), and the ISO values the message path stores parse
    // under any culture.
    internal static string FormatStoredDate(string? value)
    {
        var date = DateTime.TryParse(value, LegacyHostCulture.Culture, DateTimeStyles.None, out var parsed) ? parsed : default;
        var result = LegacyHostCulture.ToShortDate(date);
        return result;
    }

    private static FieldDataDto? MapDateOfDeclarationField(CertificateOfOriginWebDetailDto detail)
    {
        if (!string.IsNullOrWhiteSpace(detail.Value) &&
            DateTime.TryParse(detail.Value, LegacyHostCulture.Culture, DateTimeStyles.None, out var dateOfDeclaration))
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

    // Legacy SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldId).EnglishName — Lookups.DataDictionaryField.
    // TODO(internal): the three web-query row titles (fields 20306 / 20310 / 20661) come from the DataDictionary field
    // catalogue — move to the platform Lookup.DataDictionaryField and confirm its source service; until then the local stub
    // serves placeholder titles ("Field 20306"), not the real catalogue text.
    private async Task<Dictionary<int, string?>> GetFieldLabels(List<int> fieldIds)
    {
        var fields = await lookupUtil.Search<Lookups.DataDictionaryField>(field => fieldIds.Contains(field.Id) && field.State == CertificateOfOriginsConsts.ActiveState);
        var result = fields.ToDictionary(field => field.Id, field => field.EnglishName);
        return result;
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
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();

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
            // Legacy always overwrote the id: a null number matches no certificate, so the id is null.
            item.CertificateOfOriginId = item.CertificateNumber == null
                ? null
                : await DataLayer.GetCertificateOfOriginIdByNumber(item.CertificateNumber);
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
        var customerProxy = Resolve<ICustomerProxy>();
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
                certificate.ExporterTitle = exporter.Title;
                certificate.ExporterExternalIdNum = exporter.ExternalIdNum;
            }

            if (customersById.TryGetValue(certificate.CustomesAgentId, out var agent))
            {
                certificate.CustomesAgentTitle = agent.Title;
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
        var documentsProxy = Resolve<IDocumentsProxy>();

        // Legacy: SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>(CertificateTypeID).Name - see GetCertificateTypeName.
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
        var userProxy = Resolve<IUserProxy>();
        if (RequestMetadata.UserId is not int userId)
        {
            return 0;
        }

        var users = await userProxy.GetUsersByIds([userId]);
        return users?.FirstOrDefault()?.OrganizationUnit ?? 0;
    }

    // The certificate-type name (legacy SystemTablesUtil.GetCodeById<CertificateOfOriginTypeCodeEnum>(id).Name), from the
    // ECertificateOfOriginType [Display(Name)] - a recorded decision (developer, 2026-08-02; reconfirmed by the analyst for
    // parity X-2, 2026-10-08): the type table is small and changes only when a type is added, and no lookup type exists for it.
    // The enum is a copy of the table's Name column, kept in step by CertificateTypeEnumMatchesSeedTests (types 10 and 11 were
    // missing and printed as numbers). Falls back to the member name / number for an id outside the enum.
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
    // The EAI feedback-message mapping is deferred (inline TODO(blocking)).
    public async Task<CertificateOfOriginDto> SaveCertificateOfOrigin(SaveCertificateOfOriginRequestDto request)
    {
        // The SPA save passes no certificate-to-update id, exactly as the legacy internal call site did.
        var result = await SaveCertificateOfOriginCore(request, null);
        return result;
    }

    // certificateToUpdateId is legacy's SaveCertificateOfOrigin(certificateToUpdateId) argument: the id of the existing
    // certificate a CertificateUpdate message targets. Only the incoming-message path supplies it; it becomes the related
    // entity of the Received-status ApplicationCorrected event. messageContext is that path's context too: the save records
    // the lead-document title mismatch on it for the reconciliation that follows.
    private async Task<CertificateOfOriginDto> SaveCertificateOfOriginCore(SaveCertificateOfOriginRequestDto request, int? certificateToUpdateId, MessageValidationContext? messageContext = null)
    {
        // Guard the DB-required fields before anything reaches the context. Without this a malformed body (e.g. an
        // empty "{}") produced an entity with zeroed keys, the INSERT failed on a foreign key, and the
        // DbUpdateException escaped as an unhandled 500 instead of a 400.
        var validation = await new SaveCertificateOfOriginRequestValidator().ValidateAsync(request);
        if (!validation.IsValid)
        {
            // RestValidationException takes (field, message) — the same shape the CertificateId lock guard uses.
            // Report every failure in one response rather than the first, so a malformed body is fixed in one round.
            var failures = validation.Errors;
            throw new RestValidationException(
                failures[0].PropertyName,
                string.Join(" ", failures.Select(f => f.ErrorMessage)));
        }

        var userId = RequestMetadata.UserId ?? 0;
        var eventUtil = Resolve<IEventUtil>();

        var entity = BuildCertificateEntity(request);
        var details = BuildDetails(request);
        var isNewInstance = entity.Id == 0; // captured before the save assigns the real id

        // Everything that calls another service or can reject the request runs BEFORE the first write — legacy did all of
        // it before its single UoW commit, so a failure here wrote nothing (parity finding AN-01).
        if (entity.RequestReasonCode != (int)ERequestReason.CertificateCancellation)
        {
            // Generate + upload the QR: stamps QrImage, Guid and QrCodePath onto the entity so they persist with the upsert
            // (legacy CreateQRCodeIfNeededAndUpload, before the commit). The QR document is linked to no entity (AN-02), so
            // the upload does not need the certificate id.
            var qrCodeToUpload = await CreateQrCodeIfNeeded(entity);
            if (qrCodeToUpload is not null)
            {
                await UploadQrCode(entity, qrCodeToUpload);
            }

            await EnrichAndValidateDetails(entity, details);
        }

        if (entity.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.PendingRelease or (int)ECertificateOfOriginStatus.Published)
        {
            entity.ApproveUserId = userId;
        }

        var (replacementOldId, previous, previousCancelReason) = await PersistCertificateInOneTransaction(entity, details, request.CertificateOfOriginInvoiceDetails, isNewInstance);

        // Supersede events — raised here (not in SupersedePreviousVersion) because ApplicationCorrected references the
        // new certificate's id, which is only assigned by the save above. Legacy raised both when a previous existed.
        if (previous is not null)
        {
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginApplicationCorrected, entity, null);

            // Legacy scoped this event to the CANCELLED certificate's own VirtualEntity (its org unit) with its reject
            // reason as AdditionalInfo — not the new certificate's org unit / a null reason.
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginUserCancelledCertificate, previous, previousCancelReason);
        }

        // Link the DealFile lead document (repoint from the superseded certificate to this one).
        await LinkLeadDocument(entity, replacementOldId, userId, messageContext);

        // Status-change side effects. Legacy CheckIfStatusChangedAndHandleChanges: the self-tracking entity records an
        // original status only when the status was changed, and a recorded original compares against the current one.
        // Without one (a new instance, or an unchanged status) the save counts as a status change only when the status is
        // Received - so an unchanged Received certificate re-fires its events, as in production. Legacy also had an isRemarksChanged branch, but it could never be true in production: the
        // self-tracking CertificateOfOrigin records no original value for FeedbackRemark (its setter does not call
        // RecordOriginalValue), so the original was always null. A remark change therefore sends no feedback here either
        // (analyst decision 2026-09-29).
        var isStatusChanged = (!isNewInstance
                && request.OriginalCertificateOfOriginStatusId is int originalStatusId
                && originalStatusId != entity.CertificateOfOriginStatusId)
            || entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Received;

        var isAutoPublished = false;
        if (isStatusChanged)
        {
            await RaiseStatusEvents(entity, eventUtil, certificateToUpdateId);

            // Legacy ran this inside the status events (RaiseEventUtil.RaiseEventCertificateOfOriginUserApprovedCertificate),
            // so it comes before the feedback gate below. The service partial's own call (CertificateOfOriginsInternalServicePartial.cs:61-64)
            // never fired a second time: SaveCertificateOfOrigin returned the same instance, already Published by then.
            if (entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.PendingRelease)
            {
                isAutoPublished = await PublishIfDeclarationAlreadyReleased(entity);
            }
        }

        // TODO(blocking) audited gaps vs legacy SaveCertificateOfOrigin (#1 is blocked on the platform):
        //  (#1) On Publish, legacy CreateAttacmentsAndSendFeedBackMessage sent the customer a request-feedback message
        //       carrying the rendered certificate PDF (SendRequestFeedback(cert, attachments), CertificateOfOriginsBL.cs:397-418).
        //       It is NOT sent here — the feedback gate below excludes Published, and the outgoing-message (EAI) channel
        //       that carried the PDF was removed from the platform with no replacement (see SendRequestFeedback). Restore
        //       when the channel returns.
        //  (#3) Not a gap: legacy IsCreateAttachments / IsMessageSent (CertificateOfOriginsBL.cs:381,417,424,442) were
        //       in-memory properties on the entity partial (CertificateOfOriginPartial.cs:149-185), never DB columns.
        //       They only stopped a second publish within the SAME request, after the auto-publish flipped the object to
        //       Published; a retried save was never guarded. isAutoPublished below plays that role.
        // An auto-published certificate is Published by now, so — as in legacy — it gets no PendingRelease feedback.
        if (isStatusChanged && entity.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published)
        {
            await SendRequestFeedback(entity);
        }

        // Legacy reached its Published branch again after an auto-publish: the attachments were skipped by IsCreateAttachments,
        // but HandleCertificateReplacement ran a second time (a second CertificateReplaced event for the same replacement).
        // DeclarationReleased already did both, so neither is repeated here.
        if (isStatusChanged && !isAutoPublished && entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Published)
        {
            await PublishAttachments(entity, eventUtil, userId, request.IsDeclarationReleased);
            if (entity.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
            {
                await HandleCertificateReplacement(entity, eventUtil);
            }
        }

        return await GetCertificateOfOriginById(entity.Id);
    }

    // One transaction for the writes legacy committed together: cancelling the superseded version, the certificate upsert
    // and its child rows. A failure in any of them leaves the previous version as it was — never cancelled without a
    // successor (parity finding AN-01). Returns what SupersedePreviousVersion returned (zeros/nulls when nothing was superseded).
    private async Task<(int ReplacementOldId, CertificateOfOrigin? Previous, string? PreviousCancelReason)> PersistCertificateInOneTransaction(
        CertificateOfOrigin entity, List<CertificateOfOriginDetails> details, List<CertificateOfOriginInvoiceDetail> invoices, bool isNewInstance)
    {
        (int ReplacementOldId, CertificateOfOrigin? Previous, string? PreviousCancelReason) superseded = (0, null, null);
        var attempt = new SaveAttemptSnapshot(entity, details, invoices);
        var isRetry = false;
        await InTransaction(async () =>
        {
            // A transient failure retries this whole block after the rollback. The previous attempt left the ids, the row
            // version and the tracked entries it assigned, which would turn the retry into a 409; a retry starts from the
            // values the request carried. The first attempt clears nothing (a caller may have staged rows to commit here).
            if (isRetry)
            {
                DataLayer.DbContext.ChangeTracker.Clear();
                attempt.Restore();
            }

            isRetry = true;

            // New instance (not a cancellation): supersede the latest existing certificate with the same number.
            superseded = (0, null, null);
            if (isNewInstance && entity.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Cancelled)
            {
                superseded = await SupersedePreviousVersion(entity);
            }

            // Persist the certificate through BaseBL (ICloudEntity — the audit columns are stamped server-side from
            // RequestMetadata by SetEntityFields, and UpdateEntity marks CreateDate/CreateUserId not-modified so a
            // round-tripped DTO cannot overwrite them; see C12). Then diff-merge the detail rows and the invoice/item
            // graph when supplied by the incoming-message create branch (the SPA save path sends no invoices).
            if (entity.Id == 0)
            {
                AddEntity(entity);
                AuditUserStamp.ForInsert(entity, RequestMetadata.UserId);
            }
            else
            {
                UpdateEntity(entity);
                AuditUserStamp.ForUpdate(entity, RequestMetadata.UserId);
            }

            await SaveChangesAsync();

            await PersistCertificateChildren(entity.Id, details, invoices);
        });

        return superseded;
    }

    // The child rows are staged by the DAL and committed here, so a concurrency conflict raised during a child
    // merge maps to 409 exactly like one on the parent row — BaseBL.SaveChangesAsync owns that mapping, and the
    // DAL calling Context.SaveChangesAsync itself is what let those conflicts escape as an unhandled 500.
    // The invoice save must land before the items are staged: EF assigns the invoice ids there.
    private async Task PersistCertificateChildren(int certificateId, List<CertificateOfOriginDetails> details, List<CertificateOfOriginInvoiceDetail> invoices)
    {
        await DataLayer.StageCertificateOfOriginDetails(certificateId, details);
        await SaveChangesAsync();

        if (invoices.Count == 0)
        {
            return;
        }

        await DataLayer.StageCertificateOfOriginInvoices(certificateId, invoices);
        await SaveChangesAsync();

        await DataLayer.StageCertificateOfOriginInvoiceItems(invoices);
        await SaveChangesAsync();
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
    // cancelled; previous — the previous version itself, null when there was none). The supersede events are
    // NOT raised here: ApplicationCorrected references the new certificate's id, which is only assigned by the save
    // that runs after this method — so the caller raises them post-save.
    private async Task<(int ReplacementOldId, CertificateOfOrigin? Previous, string? PreviousCancelReason)> SupersedePreviousVersion(CertificateOfOrigin entity)
    {
        var previous = await DataLayer.GetLatestCertificateByNumber(entity.CertificateNumber);
        if (previous is null)
        {
            entity.VersionNumber = 1;
            entity.IsLastVersion = true;
            return (0, null, null);
        }

        // Legacy cancels the previous version + clears its IsLastVersion UNCONDITIONALLY; only the replacement-id link
        // is guarded by "not already cancelled". Guarding the whole cancel would leave an already-cancelled previous
        // still flagged IsLastVersion=true → two last-version rows for the number.
        // "\n<update received>" — the legacy appended EServerTerms.CertificateUpdateRecived.
        // The resulting reason (prior + suffix) is also the UserCancelledCertificate event's AdditionalInfo, and the
        // cancelled certificate's own OrganizationUnitId scopes that event (legacy VirtualEntity(certificateToCancel)).
        var previousCancelReason = (previous.RejectCancelReason ?? string.Empty) + Environment.NewLine + ErrorMessagesResources.CertificateUpdateRecived;
        await DataLayer.CancelPreviousCertificate(previous.Id, Environment.NewLine + ErrorMessagesResources.CertificateUpdateRecived, RequestMetadata.UserId ?? 0);

        entity.VersionNumber = previous.VersionNumber + 1;
        entity.IsLastVersion = true;

        var replacementOldId = previous.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Cancelled ? previous.Id : 0;
        return (replacementOldId, previous, previousCancelReason);
    }

    // Legacy CreateQRCodeIfNeededAndUpload (generation half) — on publish (and only when no QR path yet), generate the
    // QR from the certificate's query URL and stamp QrImage + Guid. The upload is UploadQrCode (save path, before the
    // upsert) / UploadQrCodeDocument (release path, with its own write). Returns the freshly-generated QR bytes to upload,
    // or null when nothing was created (path already set / not Published / empty CreateQrCode result).
    private async Task<byte[]?> CreateQrCodeIfNeeded(CertificateOfOrigin entity)
    {
        var commonServicesProxy = Resolve<ICommonServicesProxy>();
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

    // Legacy CreateQRCodeIfNeededAndUpload (upload half), release path (DeclarationReleased): upload the generated QR image
    // (legacy DocumentRepositoryUtil.UploadFile(fileName, Other, -1, stream) → GetDocumentFile) and persist QrCodePath with
    // its own write — this path has no main upsert (the Guid and image get their own write in UpdateCertificateQrCode).
    // The save path calls UploadQrCode before its upsert instead, so QrCodePath persists with the certificate.
    //
    // The image is NOT linked to the certificate. Legacy uploaded a bare file-server file, attached to no entity, so the
    // publish step that replaces the certificate's documents (GetDocumentsByEntity + DeleteDocuments, inline and in the
    // IssueCertificateOfOrigin worker) never saw it. Linked to the certificate, that same step deleted the QR document the
    // publish had just created, leaving QrCodePath on a deleted file (parity finding AN-02). IDocumentUtil has no bare
    // file upload, so this is a document with no entity.
    private async Task UploadQrCodeDocument(CertificateOfOrigin entity, byte[] qrBytes, int userId)
    {
        await UploadQrCode(entity, qrBytes);
        await DataLayer.UpdateCertificateQrCodePath(entity.Id, entity.QrCodePath, userId);
    }

    // Uploads the QR image and stamps QrCodePath on the entity (not persisted here).
    private async Task UploadQrCode(CertificateOfOrigin entity, byte[] qrBytes)
    {
        var documentUtil = Resolve<IDocumentUtil>();
        var organizationUnitId = await GetCurrentUserOrganizationUnitId();
        var fileName = SanitizeFileName(documentUtil, entity.CertificateNumber + ".jpg");
        var document = documentUtil.CreateDocumentBuilder()
            .WithFileName(fileName)
            .WithTitle(entity.CertificateNumber)
            .WithContent(qrBytes)
            .WithTypeId(DocumentType.Other)
            .WithOrganizationUnitId(organizationUnitId)
            .Build();
        var response = await documentUtil.UploadDocument(document);

        // QRCodePath is the QR image source of the printed certificate (usp_Template_INNER_CROSS_CertificateOfOrigin,
        // Rpt.CertificateOfOrigin_GeneralData). Legacy stored GetDocumentFile(serverRelativeUrl): the file server's URL of the
        // uploaded file. GetFileUrl builds the same URL from the uploaded file's relative resource (parity finding A-20).
        // TODO(confirm live): not verified against a real Documents service — check that it accepts a document with no
        // entity, that the stored URL opens the QR image, and that the printed certificate (SSRS) shows it, also after a
        // re-publish. No real Documents service is reachable yet.
        entity.QrCodePath = documentUtil.GetFileUrl(response.FileResource).ToString();
    }

    // The keys and the row version a save attempt assigns (the certificate, its detail rows, its invoices and their items),
    // as the request carried them, so a retried attempt can start over (see PersistCertificateInOneTransaction).
    private sealed class SaveAttemptSnapshot
    {
        private readonly CertificateOfOrigin entity;
        private readonly int id;
        private readonly byte[] timeStamp;
        private readonly List<(CertificateOfOriginDetails Row, int Id)> details;
        private readonly List<(CertificateOfOriginInvoiceDetail Row, int Id)> invoices;
        private readonly List<(CertificateOfOriginItemDetail Row, int Id)> items;

        public SaveAttemptSnapshot(CertificateOfOrigin entity, List<CertificateOfOriginDetails> details, List<CertificateOfOriginInvoiceDetail> invoices)
        {
            this.entity = entity;
            id = entity.Id;
            timeStamp = entity.TimeStamp;
            this.details = details.Select(row => (row, row.Id)).ToList();
            this.invoices = invoices.Select(row => (row, row.Id)).ToList();
            items = invoices.SelectMany(invoice => invoice.CertificateOfOriginItemDetail).Select(row => (row, row.Id)).ToList();
        }

        public void Restore()
        {
            entity.Id = id;
            entity.TimeStamp = timeStamp;
            details.ForEach(saved => saved.Row.Id = saved.Id);
            invoices.ForEach(saved => saved.Row.Id = saved.Id);
            items.ForEach(saved => saved.Row.Id = saved.Id);
        }
    }

    // Runs the action in one database transaction on the write context. The platform enables EF's retrying execution
    // strategy, which refuses a user-initiated transaction outside the strategy, so the transaction runs inside it (a
    // transient failure retries the whole unit). Only database work belongs in the action — no calls to other services.
    private async Task InTransaction(Func<Task> action)
    {
        var database = DataLayer.DbContext.Database;
        var strategy = database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.BeginTransactionAsync();
            await action();
            await transaction.CommitAsync();
        });
    }

    // The subset of country types whose value is also checked for trade-agreement membership (legacy
    // CheckIfCountryIsInTradeAgreement / CheckAgreementFirstCountry).
    private static readonly HashSet<int> TradeAgreementCountryDetailTypes =
    [
        (int)ECertificateDetailsType.TradeAgreementCountry1, (int)ECertificateDetailsType.TradeAgreementCountry2,
        (int)ECertificateDetailsType.ConsigneeCountry, (int)ECertificateDetailsType.OriginCountry,
        (int)ECertificateDetailsType.CumulationCountry, (int)ECertificateDetailsType.DestinationCountry,
    ];

    // Country-group detail types whose id → English name is resolved for display (legacy CheckIfCountryGroupIsInTradeAgreement).
    private static readonly HashSet<int> CountryGroupDetailTypes =
    [
        (int)ECertificateDetailsType.TradeAgreementGroupOfCountries, (int)ECertificateDetailsType.OriginGroupOfCountries,
        (int)ECertificateDetailsType.DestinationGroupOfCountries, (int)ECertificateDetailsType.CumulationGroupOfCountries,
    ];

    private static readonly HashSet<int> CityDetailTypes =
    [
        (int)ECertificateDetailsType.CityOfDeclaration, (int)ECertificateDetailsType.PlaceOfManufacture,
    ];

    // International-site id detail types (legacy CheckIfSiteExist; PortOfShipment via CheckIfInternationalSiteExist, see below).
    private static readonly HashSet<int> SiteIdDetailTypes =
    [
        (int)ECertificateDetailsType.ExportPort, (int)ECertificateDetailsType.PortOfEntrance, (int)ECertificateDetailsType.ExitPort,
        (int)ECertificateDetailsType.PortOfShipment,
    ];

    // The free-text detail types legacy CheckSpecificField displayed as entered (DisplayedValue = Value).
    private static readonly HashSet<int> TextDetailTypes =
    [
        (int)ECertificateDetailsType.ExporterName, (int)ECertificateDetailsType.ExporterAddress,
        (int)ECertificateDetailsType.ConsigneeName, (int)ECertificateDetailsType.ConsigneeAddress,
        (int)ECertificateDetailsType.ConsigneeRemarks, (int)ECertificateDetailsType.Transport,
        (int)ECertificateDetailsType.ZipCodeOfManufacture, (int)ECertificateDetailsType.Observations,
        (int)ECertificateDetailsType.Feedback, (int)ECertificateDetailsType.ImportBillOfLadingNum,
        (int)ECertificateDetailsType.GoodsDescription, (int)ECertificateDetailsType.DeclaringCompany,
        (int)ECertificateDetailsType.DeclaringPerson, (int)ECertificateDetailsType.DeclaringPosition,
        (int)ECertificateDetailsType.ManifestNum, (int)ECertificateDetailsType.ExportBillOFLadingNum,
    ];

    // Bool detail types whose display is Yes/No (legacy CheckSpecificField → ConvertBoolFieldToYesNo).
    private static readonly HashSet<int> BoolDetailTypes =
    [
        (int)ECertificateDetailsType.IsConsigneeForPrint, (int)ECertificateDetailsType.IsCumulation,
        (int)ECertificateDetailsType.IsExportDecForPrint, (int)ECertificateDetailsType.IsDeclaredByManufacturer,
        (int)ECertificateDetailsType.IsDeclaredByExporter,
    ];

    // Legacy new-instance branch (CertificateOfOriginsBL.cs:984-1000): a brand-new certificate's DestinationCountry /
    // PortOfShipment details arrive from the SPA as an alpha-2 country code / locode; resolve them to the internal id
    // (Country by alpha-2, InternationalSite by locode). An unknown code is a 400, as legacy failed the save on it.
    private async Task ResolveNewInstanceDetailCodes(List<CertificateOfOriginDetails> details)
    {
        foreach (var detail in details)
        {
            if (string.IsNullOrEmpty(detail.Value))
            {
                continue;
            }

            // Legacy SystemTablesUtil.GetIdByCode threw TheValueInFieldNotExistsInSystem for a code it did not find, so the
            // save failed with nothing written (parity finding AN-04). This runs before the first write (AN-01).
            // TODO(confirm): the message text. Legacy filled {0} with the table's DataDictionaryTable.Description (Hebrew) —
            // IdByCodeLogicGetter.cs:77-79 — which lives in the infrastructure DB; the field name stands in until it is known.
            if (detail.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.DestinationCountry)
            {
                var country = (await lookupUtil.Search<Lookup.Country>(c => c.CountryAlphaCode2 == detail.Value && c.State == CertificateOfOriginsConsts.ActiveState)).FirstOrDefault()
                    ?? throw new RestValidationException(nameof(ECertificateDetailsType.DestinationCountry), $"הערך בשדה {nameof(ECertificateDetailsType.DestinationCountry)} לא קיים במערכת");
                detail.Value = country.Id.ToString(CultureInfo.InvariantCulture);
            }
            else if (detail.CertificateDetailsTypeCodeId == (int)ECertificateDetailsType.PortOfShipment)
            {
                var site = (await lookupUtil.Search<Lookups.InternationalSite>(s => s.Locode == detail.Value && s.State == CertificateOfOriginsConsts.ActiveState)).FirstOrDefault()
                    ?? throw new RestValidationException(nameof(ECertificateDetailsType.PortOfShipment), $"הערך בשדה {nameof(ECertificateDetailsType.PortOfShipment)} לא קיים במערכת");
                detail.Value = site.Id.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    private async Task EnrichAndValidateDetails(CertificateOfOrigin entity, List<CertificateOfOriginDetails> details)
    {
        var customerProxy = Resolve<ICustomerProxy>();

        // Legacy SaveCertificateOfOrigin (CertificateOfOriginsBL.cs:977-1000): a NEW certificate only converts its
        // DestinationCountry / PortOfShipment codes to ids; the CheckSpecificField enrichment + validation below runs for
        // an EXISTING one. The message path already validated and resolved its new certificate's fields
        // (ValidateMessageField), so running the loop again would re-resolve an already-internal exporter id as an
        // external one. entity.Id == 0 is captured before the upsert assigns the real id.
        if (entity.Id == 0)
        {
            await ResolveNewInstanceDetailCodes(details);
            return;
        }

        foreach (var detail in details)
        {
            var value = detail.Value;
            var typeId = detail.CertificateDetailsTypeCodeId;
            if (typeId == (int)ECertificateDetailsType.ExporterId)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var exporterId = await customerProxy.GetCustomerIdByExternalId(value);
                    if (exporterId is int resolvedExporterId && resolvedExporterId != 0)
                    {
                        detail.Value = resolvedExporterId.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            else if (typeId == (int)ECertificateDetailsType.CustomsHouse)
            {
                // Legacy CheckIfSiteExistAndCustomsHouse → CheckIfCustomsHouse: DisplayedValue = org-unit English name.
                // By this point Value is the resolved org-unit id (the message path rewrote it; the direct endpoint sends
                // it), so resolve the name from the id — falling back to the raw value when the lookup is unavailable.
                if (int.TryParse(value, out var orgUnitId))
                {
                    var organizationUnit = await lookupUtil.Get<Lookup.OrganizationUnit>(orgUnitId);
                    detail.DisplayedValue = organizationUnit?.EnglishName ?? value;
                }
                else
                {
                    detail.DisplayedValue = value;
                }
            }
            else
            {
                await ApplyLegacyDisplayedValue(entity, detail);
            }
        }
    }

    // The DisplayedValue that legacy CheckSpecificField gave the detail of an EXISTING certificate, CertificateOfOriginsBL.cs lines 1452-1545,
    // case by case. A case legacy did not touch leaves the client-sent DisplayedValue as it is: the four Israel-only country
    // types, an out-of-range date, a non-numeric id and any type the switch does not list. Parity finding AN-03.
    // The certificate report (the GeneralData procedure) prints DisplayedValue, so a rewrite here changes the printed text.
    private async Task ApplyLegacyDisplayedValue(CertificateOfOrigin entity, CertificateOfOriginDetails detail)
    {
        var value = detail.Value;
        var typeId = detail.CertificateDetailsTypeCodeId;
        var today = DateTime.Today;

        if (BoolDetailTypes.Contains(typeId))
        {
            // Legacy ConvertBoolFieldToYesNo.
            detail.DisplayedValue = bool.TryParse(value, out var boolValue) && boolValue ? "Yes" : "No";
        }
        else if (typeId == (int)ECertificateDetailsType.DateOfDeclaration)
        {
            // Legacy CheckDeclarationDate: the short date only when today-5 days <= date <= today.
            SetShortDateWhenInRange(detail, today.AddDays(-5), today);
        }
        else if (typeId is (int)ECertificateDetailsType.ExportDate or (int)ECertificateDetailsType.ExpectedExitDate)
        {
            // Legacy CheckExportDate / CheckExpectedExitDate: the short date only when today <= date <= today+3 months.
            SetShortDateWhenInRange(detail, today, today.AddMonths(3));
        }
        else if (typeId == (int)ECertificateDetailsType.ImportDate)
        {
            // Legacy CheckImportDate: always the short date (an unparseable value yields default(DateTime)).
            detail.DisplayedValue = FormatStoredDate(value);
        }
        else if (TradeAgreementCountryDetailTypes.Contains(typeId) && int.TryParse(value, out var countryId))
        {
            // Legacy CheckIfCountryIsInTradeAgreement: the trade-agreement check (its exceptions never reach the save), then
            // the country's English name.
            await IsTradeAgreementForCountry(entity.TypeId, countryId, false);
            var country = await lookupUtil.Get<Lookup.Country>(countryId, CertificateOfOriginsConsts.ActiveState);
            SetDisplayedValueWhenFound(detail, country?.EnglishName);
        }
        else if (CityDetailTypes.Contains(typeId) && int.TryParse(value, out var cityId))
        {
            // Legacy CheckCityOfDeclaration.
            var city = await lookupUtil.Get<Lookup.City>(cityId, CertificateOfOriginsConsts.ActiveState);
            SetDisplayedValueWhenFound(detail, city?.EnglishName);
        }
        else if (CountryGroupDetailTypes.Contains(typeId) && int.TryParse(value, out var countryGroupId))
        {
            // Legacy CheckIfCountryGroupIsInTradeAgreement: the group's English name.
            var countryGroup = await lookupUtil.Get<Lookups.CountryGroup>(countryGroupId, CertificateOfOriginsConsts.ActiveState);
            SetDisplayedValueWhenFound(detail, countryGroup?.EnglishName);
        }
        else if (typeId == (int)ECertificateDetailsType.ExportCountry && int.TryParse(value, out var exportCountryId))
        {
            // Legacy CheckExportCountry (CertificateOfOriginsBL.cs:1857-1874): the Israel checks (never thrown on the save),
            // then the country's English name.
            var country = await lookupUtil.Get<Lookup.Country>(exportCountryId, CertificateOfOriginsConsts.ActiveState);
            SetDisplayedValueWhenFound(detail, country?.EnglishName);
        }
        else if (SiteIdDetailTypes.Contains(typeId) && int.TryParse(value, out var siteId))
        {
            // Legacy CheckIfSiteExist: the value is an international-site id → the site's English name (parity finding A-21).
            // PortOfShipment too: legacy CheckIfInternationalSiteExist went through the private GetIdByCode wrapper, which
            // returns the parsed value itself (not the looked-up id), so a stored numeric id got the site's name and a
            // non-numeric value was left alone (CertificateOfOriginsBL.cs:1876-1905).
            var site = await lookupUtil.Get<Lookups.InternationalSite>(siteId, CertificateOfOriginsConsts.ActiveState);
            SetDisplayedValueWhenFound(detail, site?.EnglishName);
        }
        else if (TextDetailTypes.Contains(typeId))
        {
            // Legacy: the free-text types are displayed as entered.
            detail.DisplayedValue = value;
        }
    }

    private static void SetShortDateWhenInRange(CertificateOfOriginDetails detail, DateTime from, DateTime to)
    {
        var date = DateTime.TryParse(detail.Value, LegacyHostCulture.Culture, DateTimeStyles.None, out var parsed) ? parsed : default;
        if (date >= from && date <= to)
        {
            detail.DisplayedValue = LegacyHostCulture.ToShortDate(date);
        }
    }

    // Legacy dereferenced the lookup row directly (a missing row crashed the save); here a missing row leaves the display.
    private static void SetDisplayedValueWhenFound(CertificateOfOriginDetails detail, string? englishName)
    {
        if (englishName is not null)
        {
            detail.DisplayedValue = englishName;
        }
    }

    // Legacy: repoint the DealFile lead document to this certificate; backfill LeadDocumentId/ExportDeclarationNumber
    // when the certificate has none; then CheckDeclarationStatus.
    private async Task LinkLeadDocument(CertificateOfOrigin entity, int replacementOldId, int userId, MessageValidationContext? messageContext)
    {
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var oldId = replacementOldId != 0 ? replacementOldId : entity.Id;
        var leadDocument = await exportDealFileProxy.GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(oldId, entity.Id);

        // Legacy title check (CertificateOfOriginsBL.cs:1014-1017): the lead document's title against the certificate's
        // export-declaration number, compared BEFORE the backfill below — so a certificate with no number counts as a
        // mismatch when the title is set, as in legacy. Legacy added it to the BL's _requestExceptions, which only the
        // incoming message's reconciliation read; the internal save discarded it. Hence it is recorded on the message
        // context only.
        if (leadDocument is not null && messageContext is not null && leadDocument.LeadDocumentTitle != entity.ExportDeclarationNumber)
        {
            messageContext.IsLeadDocumentTitleMismatch = true;
        }

        if (leadDocument is not null && string.IsNullOrEmpty(entity.ExportDeclarationNumber))
        {
            entity.LeadDocumentId = leadDocument.LeadDocumentId;
            entity.ExportDeclarationNumber = leadDocument.LeadDocumentTitle;

            // The backfill is stamped after the main upsert (this method needs the new certificate id), so it needs its
            // own explicit write to persist.
            await DataLayer.UpdateCertificateDeclarationLink(entity.Id, entity.LeadDocumentId, entity.ExportDeclarationNumber, userId);
        }

        // Legacy CheckDeclarationStatus(certificate, false) (CertificateOfOriginsBL.cs:648-667): when the certificate
        // carries a declaration and either replaces a certificate (replacementOldId - legacy CertificateOfOriginIdOfReplacement)
        // or has an earlier version (the second-newest with the same Title, any status), repoint the declaration's lead
        // document from that certificate to this one.
        var declarationDetails = entity.LeadDocumentId.HasValue || entity.ExportDeclarationNumber is not null
            ? await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(entity.LeadDocumentId, entity.ExportDeclarationNumber)
            : null;
        if (declarationDetails is null)
        {
            return;
        }

        var previousCertificateId = replacementOldId != 0 ? replacementOldId : await DataLayer.GetPreviousCertificateIdByTitle(entity.Title);
        if (previousCertificateId.HasValue)
        {
            await exportDealFileProxy.ChangeCertificateOfOriginIdForLeadDocument(declarationDetails.LeadDocumentId, previousCertificateId.Value, entity.Id);
        }
    }

    // Legacy RaiseEventCertificateOfOriginUserApprovedCertificate (RaiseEventUtil.cs:165-196, run for a status change to
    // PendingRelease): when the certificate's export declaration is ALREADY released (lead-document state Released, Paroled
    // or Closed), publish it at once through DeclarationReleased, exactly as the release event would. Returns true when it
    // published; the entity is then marked Published, as legacy's shared instance was.
    // The legacy IsExportDeclarationActive gate is dropped (always true, parity B-L1). Legacy passed no organization unit
    // on this path, and the release path ignores the other declaration fields beyond the number and lead document.
    // A certificate with neither a lead document nor a declaration number has no declaration to look up; legacy queried
    // with both null here, and the lookup finds nothing.
    private async Task<bool> PublishIfDeclarationAlreadyReleased(CertificateOfOrigin entity)
    {
        if (!entity.LeadDocumentId.HasValue && string.IsNullOrEmpty(entity.ExportDeclarationNumber))
        {
            return false;
        }

        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(entity.LeadDocumentId, entity.ExportDeclarationNumber);
        if (declarationDetails is null)
        {
            return false;
        }

        var declarationInfo = await exportDealFileProxy.GetExportDeclarationInfoForPc(declarationDetails.LeadDocumentId);
        if (declarationInfo is null
            || declarationInfo.LeadDocumentState is not ((int)ELeadDocumentState.Released
                or (int)ELeadDocumentState.Paroled
                or (int)ELeadDocumentState.Closed))
        {
            return false;
        }

        // Legacy called DeclarationReleased directly (isFromDealFile:false), not the event dispatcher, so EventType stays
        // unset. Its reconciliation result was discarded (void), and so is it here.
        var releaseRequest = new UpdateCertificateOfOriginsRequestDto
        {
            CertificateOfOriginsIds = [entity.Id],
            DestinationCountryId = declarationInfo.DestinationCountryId,
            ExporterCustomerId = declarationInfo.ExporterCustomerId,
            ExportDeclarationNum = entity.ExportDeclarationNumber,
            LeadDocumentId = declarationInfo.LeadDocumentId,
            ExportInvoiceInfoList = declarationInfo.ExportInvoiceInfoList,
        };
        await DeclarationReleased(releaseRequest);

        entity.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published;
        return true;
    }

    // Legacy RaiseCertificateOfOriginEvents: the status-driven event for the new status. The legacy also raised a
    // "new certificate created" secondary event on Received / DeclarationMatch, but only when the export-declaration
    // integration was off — that branch is gone (see the note at the end of this method). The legacy
    // DeclarationMismatch "assessor decision" is a no-op (empty in the legacy), so nothing is raised there.
    private static async Task RaiseStatusEvents(CertificateOfOrigin entity, IEventUtil eventUtil, int? certificateToUpdateId)
    {
        int? specificEvent = entity.CertificateOfOriginStatusId switch
        {
            (int)ECertificateOfOriginStatus.Received => (int)EEventType.CertificateOfOriginApplicationReceived,
            (int)ECertificateOfOriginStatus.Rejected => (int)EEventType.CertificateOfOriginUserDeniedCertificate,
            (int)ECertificateOfOriginStatus.Cancelled => (int)EEventType.CertificateOfOriginUserCancelledCertificate,
            (int)ECertificateOfOriginStatus.PendingRelease => (int)EEventType.CertificateOfOriginUserApprovedCertificate,

            // Legacy RaiseEventCertificateOfOriginCertificateMatchDeclaration (CR 156814): the reasons that need no
            // assessor task raise the "without task" variant.
            (int)ECertificateOfOriginStatus.DeclarationMatch => entity.RequestReasonCode is (int)ERequestReason.EmptyCertificate
                    or (int)ERequestReason.Draft or (int)ERequestReason.GetRequestStatus or (int)ERequestReason.CertificateCancellation
                ? (int)EEventType.CertificateMatchDeclarationWithoutTask
                : (int)EEventType.CertificateOfOriginCertificateMatchDeclaration,
            (int)ECertificateOfOriginStatus.DeclarationMismatch => (int)EEventType.CertificateOfOriginCertificateDeclarationMismatch,
            _ => null,
        };
        if (specificEvent is not null)
        {
            // Legacy RaiseEventUtil attached the RejectCancelReason as the AdditionalInfo on the reject and cancel
            // events, and attached nothing on the other status events.
            var additionalInfo = entity.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.Rejected or (int)ECertificateOfOriginStatus.Cancelled
                ? entity.RejectCancelReason
                : null;

            // Legacy set the Export organization-unit type on the match and mismatch events' VirtualEntity.
            await RaiseCertificateEvent(eventUtil, specificEvent.Value, entity, additionalInfo,
                entity.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.DeclarationMatch or (int)ECertificateOfOriginStatus.DeclarationMismatch
                    ? CertificateOfOriginsConsts.ExportOrganizationUnitType
                    : null);
        }

        // Legacy RaiseEventForStatusReceived: a CertificateUpdate request raised ApplicationCorrected right after
        // ApplicationReceived. The gate is the certificate's own RequestReasonCode (not the save's requestReason
        // argument, which the incoming path passes as null), and the superseded certificate is attached as a related
        // entity only when the caller supplied its id (the SPA save does not).
        if (entity.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.Received
            && entity.RequestReasonCode == (int)ERequestReason.CertificateUpdate)
        {
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginApplicationCorrected, entity, null,
                relatedCertificateId: certificateToUpdateId);
        }

        // Legacy RaiseNewCertificateOfOriginCreatedEvent removed (developer decision 2026-08-23). It raised the
        // CertificateOfOriginNewCertificateOfOriginCreated event (assessor as preferred user) ONLY when the
        // export-declaration integration was off — i.e. when the "IsExportDeclarationActive" parameter was false.
        // That parameter is obsolete and is now always true, so the guard always returned and the event could never
        // be raised. The dead branch and its parameter read are dropped rather than seeding a flag that only ever
        // has one value. RaiseCertificatePreferredAssessorEvent itself is still used by the declaration-match flow.
    }

    // Legacy SendRequestFeedback — the certificate request-feedback message to the creating customer.
    // 🛑 TODO(blocking): legacy delivered this as an EAI OUTGOING message (OutgoingMessageProxy →
    // PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback) carrying the rendered certificate PDF as an Attachment. That
    // outgoing-message channel is REMOVED from the platform (InfrastructureCore.Utils ≥ 1.10.105) with no replacement,
    // so it was substituted with IMessageManagementProxy (a different, internal channel). Two gaps remain and need a
    // platform/product decision, NOT a local fix: (1) SendMessageDto has no attachment field, so the rendered PDF is not
    // delivered at all; (2) MessageTypeId is unmapped (0). Do not fabricate an attachment field until the outgoing-message
    // channel returns or MessageManagement's attachment contract is confirmed.
    private async Task SendRequestFeedback(CertificateOfOrigin entity)
    {
        var messageManagementProxy = Resolve<IMessageManagementProxy>();
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
    private async Task PublishAttachments(CertificateOfOrigin entity, IEventUtil eventUtil, int userId, bool? isDeclarationReleased)
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

        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateIssued, entity, null);

        // Issue-by-worker: publish the certificate to the RabbitMQ issue queue instead of generating the template inline.
        // Legacy issued through the worker only when the certificate type has an SSRS ReportId (every type does).
        if (issueByWorker && entity.IsInPublishingProcess)
        {
            var reportId = (await DataLayer.GetCertificateTypeCode(entity.TypeId))?.ReportId;
            if (reportId.HasValue)
            {
                await SendCertificateToIssueQueue(entity, reportId.Value, isDeclarationReleased);
            }

            return;
        }

        // Inline: generate the certificate template and save it as an attachment (legacy PrintCertificateOfOriginAndSaveAttachments).
        await PrintCertificateOfOriginAndSaveAttachments(entity, string.Empty);
    }

    // Legacy PrintCertificateOfOriginAndSaveAttachments: render the certificate document via SSRS (the Common service's
    // GenerateReportByParams) and save it as an attachment on the certificate (→ SaveCertificateOfOriginAttachments).
    // Shared by the publish flow (#33) and the reconciliation re-print (#34). Rendering is always delegated to SSRS
    // (developer decision — no per-type template switch / no local template files).
    // Returns the rendered template so the caller can put it on the message feedback (legacy returned
    // List<TemplateResult> for exactly that reason — CreateAttachments mapped it onto the response). Null when
    // nothing was rendered.
    private async Task<TemplateResultDto?> PrintCertificateOfOriginAndSaveAttachments(CertificateOfOrigin certificate, string additionalInfo)
    {
        // Legacy rendered the certificate with its type's SSRS report (CertificateOfOriginTypeCode.ReportId, 7000-7007),
        // never with the TypeId. A type without a ReportId fell to a per-type template switch that no type reaches (every
        // type has one), so it is not rendered here.
        var reportId = (await DataLayer.GetCertificateTypeCode(certificate.TypeId))?.ReportId;
        if (reportId is null)
        {
            return null;
        }

        // Legacy CommonServicesServiceAdapter.GanerateReportAndConvertToTemplateResult: GenerateReportByParams with the
        // EntityID + AdditionalInfo report parameters in PDF, wrapped as a PDF ExportCertificateOfOrigin (329) template.
        var commonServicesProxy = Resolve<ICommonServicesProxy>();
        var reportRequest = new GenerateReportByParamsRequestDto
        {
            ReportId = reportId.Value,
            ReportParameters =
            [
                new(CertificateOfOriginsConsts.ReportParameterEntityId, certificate.Id.ToString(CultureInfo.InvariantCulture)),
                new(CertificateOfOriginsConsts.ReportParameterAdditionalInfo, additionalInfo),
            ],
            FileFormat = CertificateOfOriginsConsts.ReportFileFormatPdf,
        };
        var reportContent = await commonServicesProxy.GenerateReportByParams(reportRequest);
        if (reportContent is null)
        {
            return null;
        }

        var template = new TemplateResultDto
        {
            Content = reportContent,
            IsPdfFormat = true,
            DocumentTypeId = CertificateOfOriginsConsts.ExportCertificateOfOriginDocumentTypeId,
        };

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
        return template;
    }

    // Legacy SendCertificateToIssueQueue — publish the certificate to the "IssueCertificateOfOrigin" RabbitMQ exchange
    // for asynchronous issuing by a worker (IQueueUtil, mirroring QueueUtilFactory 1:1).
    private async Task SendCertificateToIssueQueue(CertificateOfOrigin entity, int reportId, bool? isDeclarationReleased)
    {
        var payload = new IssueCertificateDto
        {
            ReportId = reportId,
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
            IsDeclarationReleased = isDeclarationReleased,
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

    // Legacy HandleCertificateReplacement (CertificateOfOriginsBL.cs:356-377) — remark on the replacing certificate, cancel
    // the replaced one + raise the replaced event scoped to the cancelled certificate's own identity / organization unit.
    // TODO(migration): the agent talk-back (legacy servicesAdapter.SendMessageToAgent with EMessages.UpdateExportDecForReplacement,
    // params {cancelled.ExportDeclarationNumber, cancelled.CertificateNumber, entity.CertificateNumber}) waits for the
    // agent-message proxy operation; the text is ErrorMessagesResources.UpdateExportDecForReplacement.
    private async Task HandleCertificateReplacement(CertificateOfOrigin entity, IEventUtil eventUtil)
    {
        if (entity.CertificateIdToCancel is null)
        {
            return;
        }

        // Legacy fetched the replaced certificate and no-op'd when it was missing; its own org unit scopes the event.
        var certificateToCancel = (await DataLayer.GetCertificatesByIds([entity.CertificateIdToCancel.Value])).FirstOrDefault();
        if (certificateToCancel is null)
        {
            return;
        }

        var userId = RequestMetadata.UserId ?? 0;

        // Legacy EMessages.UpdateExportDeclaration: the replacing certificate's remark asks the agent to update the export
        // declaration; saved with the certificate (legacy Repository.Save(certificate) at the end of the method).
        entity.FeedbackRemark = string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.UpdateExportDeclaration, entity.ExportDeclarationNumber, certificateToCancel.CertificateNumber);
        await DataLayer.UpdateCertificateFeedbackRemark(entity.Id, entity.FeedbackRemark, userId);

        // Legacy: Cancelled + the CertificateReplaced reason (replacing the old one). The replaced certificate keeps
        // IsLastVersion - a replacement has a different number, so it is still the last version of its own.
        await DataLayer.CancelCertificate(certificateToCancel.Id, string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.CertificateReplaced, entity.CertificateNumber), userId);

        // Scope the replaced event to the CANCELLED certificate's own organization unit (legacy VirtualEntity(certificateToCancel)),
        // not the replacement certificate's — the two can differ on a cross-organization import-certificate replacement.
        await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateReplaced, certificateToCancel, entity.CertificateNumber);
    }

    // A certificate-scoped event (VirtualEntity(certificate)) with an optional AdditionalInfo.
    // The header every certificate event shares. Legacy built them from new VirtualEntity(certificate), whose constructor
    // copied the certificate's Title (its number) and CustomerID onto the event, so the task title shows the certificate
    // number and the event is linked to the customer.
    private static IEventRequestBuilder CreateCertificateEventBuilder(IEventUtil eventUtil, int eventTypeId, CertificateOfOrigin certificate)
    {
        var builder = eventUtil.CreatBuilder()
            .WithEventType(eventTypeId)
            .WithEntity((int)EEntityType.CertificateOfOrigin, certificate.Id)
            .WithTitle(certificate.Title)
            .WithCustomerId(certificate.CustomerId.ToString(CultureInfo.InvariantCulture));

        // Legacy EventUtil.RaiseEvent tolerated a 0/absent org unit — a NonManipulation certificate with no customs
        // house is saved with OrganizationUnitID 0 (legacy _organizationUnit default). The .NET 10 builder rejects 0,
        // so apply it only when present, preserving the legacy behaviour (the event is still raised).
        if (certificate.OrganizationUnitId > 0)
        {
            builder = builder.WithOrganizationUnitId(certificate.OrganizationUnitId);
        }

        return builder;
    }

    // Legacy built this one event's VirtualEntity by hand (an initializer, not the copying constructor): the id, the org
    // unit and a fixed title - no customer.
    private static async Task RaiseImportReplacementTaskEvent(IEventUtil eventUtil, CertificateOfOrigin certificate)
    {
        var builder = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate)
            .WithEntity((int)EEntityType.CertificateOfOrigin, certificate.Id)
            .WithTitle(nameof(EEventType.OpenTaskHandlingTheReplacementOfAnImportCertificate));
        if (certificate.OrganizationUnitId > 0)
        {
            builder = builder.WithOrganizationUnitId(certificate.OrganizationUnitId);
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    private static async Task RaiseCertificateEvent(IEventUtil eventUtil, int eventTypeId, CertificateOfOrigin certificate, string? additionalInfo, int? organizationUnitTypeId = null, int? relatedCertificateId = null)
    {
        var builder = CreateCertificateEventBuilder(eventUtil, eventTypeId, certificate);

        // Legacy set the organization-unit TYPE (Export) on the declaration match/mismatch events' VirtualEntity — passed
        // only by those callers (the save's status events and the reconciliation mismatch); other events leave it unset.
        if (organizationUnitTypeId.HasValue)
        {
            builder = builder.WithOrganizationUnitTypeId((CustomsCloud.InfrastructureCore.Interfaces.Shared.OrganizationUnitTypes)organizationUnitTypeId.Value);
        }

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            builder = builder.WithAdditionalInfo(additionalInfo);
        }

        if (relatedCertificateId.HasValue)
        {
            builder = builder.AddRelatedEntity(relatedCertificateId.Value, (int)EEntityType.CertificateOfOrigin);
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Internal WCF: UpdateCetrificateOfOrigins(dto) — a 5-way EEventType dispatcher over the DealFile export-declaration
    // events (legacy InternalUpdateCetrificateOfOrigins's switch). Each event routes to its own handler; only the
    // submission-succeeded case reconciles and returns errors, so the other branches return an empty list.
    // Legacy opened with `if (!IsExportDeclarationActive) return;` — that parameter is obsolete and always true (the same
    // developer decision that dropped the RaiseNewCertificateOfOriginCreated guard, 2026-08-23), so the gate is a no-op
    // and is not reproduced. Decided (parity B-L1, 2026-10-08): the gate stays dropped - the analyst confirmed the flag is
    // permanently true (2026-09-23), and the three other legacy places it gated are dropped on the same premise.
    public async Task<List<CertificateOfOriginExceptionDto>> UpdateCertificateOfOrigins(UpdateCertificateOfOriginsRequestDto request)
    {
        switch (request.EventType)
        {
            case (int)EEventType.ExportDeclarationSubmissionSucceeded:
                return await ReconcileCertificatesAgainstDeclaration(request);
            case (int)EEventType.ExportDeclarationReleased:
            case (int)EEventType.AssemblySharedReleaseAccepted:
                return await DeclarationReleased(request);
            case (int)EEventType.ExportDeclarationAmendmentRequestCompleted:
                return await ExportDeclarationAmendmentSuccess(request);
            case (int)EEventType.CancellationRequestCommited:
                await ExportDeclarationCancellationRequestCommited(request);
                return [];
            default:
                return [];
        }
    }

    // ExportDeclarationSubmissionSucceeded case — the export-declaration → certificate reconciliation (a one-way
    // DealFile event). For each certificate to reconcile: backfill the declaration link; skip the gate for a
    // non-Received / empty / status-query / cancellation request or a non-manipulation type; otherwise validate it
    // against the declaration (scalar details + destination/origin country-groups + import-replacement trade-agreement +
    // the full invoice / goods-item / customs-item 6-digit matching, both directions), then — unless it is a Draft — set
    // DeclarationMatch / Rejected / DeclarationMismatch, raise the matching event (warnings assign the mismatch task to
    // the assessor), re-print the draft, and append the error rows. Returns the reconciliation errors (texts and codes:
    // EReconciliationMessage).
    // titleMismatchCertificateId: the certificate whose save found a lead-document title mismatch (incoming message path only).
    private async Task<List<CertificateOfOriginExceptionDto>> ReconcileCertificatesAgainstDeclaration(UpdateCertificateOfOriginsRequestDto request, int? titleMismatchCertificateId = null)
    {
        var exceptions = new List<CertificateOfOriginExceptionDto>();
        if (request.CertificateOfOriginsIds == null || request.CertificateOfOriginsIds.Count == 0)
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
            // Backfill the declaration link from the DTO, faithfully to legacy (CertificateOfOriginsBL.cs lines 492-500).
            // When the certificate has no declaration number, take both the number and the lead-document from the
            // request. When it already has a number, take the request's lead-document only when that number equals the
            // request's number, so a certificate is never linked to a different declaration's lead document.
            if (string.IsNullOrEmpty(certificate.ExportDeclarationNumber))
            {
                certificate.ExportDeclarationNumber = request.ExportDeclarationNum;
                certificate.LeadDocumentId = request.LeadDocumentId;
            }

            if (!certificate.LeadDocumentId.HasValue && certificate.ExportDeclarationNumber == request.ExportDeclarationNum)
            {
                certificate.LeadDocumentId = request.LeadDocumentId;
            }

            // Legacy gate: reconcile only a still-Received certificate that is not an empty / status-query / cancellation
            // request and not a non-manipulation type. Others are only backfilled.
            if (certificate.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Received
                || certificate.RequestReasonCode is (int)ERequestReason.EmptyCertificate or (int)ERequestReason.GetRequestStatus or (int)ERequestReason.CertificateCancellation
                || certificate.TypeId == (int)ECertificateOfOriginType.NonManipulation)
            {
                await DataLayer.UpdateCertificateReconciliation(certificate.Id, certificate.CertificateOfOriginStatusId, certificate.ExportDeclarationNumber, certificate.LeadDocumentId, certificate.RejectCancelReason, userId);
                continue;
            }

            var details = detailsByCertificate.GetValueOrDefault(certificate.Id) ?? [];
            var reconciliation = await ValidateReconciliation(request, certificate, details, certificate.Id == titleMismatchCertificateId);
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

            await DataLayer.UpdateCertificateReconciliation(certificate.Id, newStatus, certificate.ExportDeclarationNumber, certificate.LeadDocumentId, rejectReason, userId);

            if (certificateExceptions.Count > 0)
            {
                // Append the mismatch-log rows (legacy item.CertificateOfOriginVsDeclarationError.Add per exception).
                await DataLayer.AddCertificateVsDeclarationErrors(certificate.Id, certificateExceptions.Select(exception => exception.ExceptionDescription ?? string.Empty).ToList());
                await SaveChangesAsync();
            }
        }

        return exceptions;
    }

    // ExportDeclarationAmendmentRequestCompleted case (legacy ExportDeclarationAmendmentSuccess): the export
    // declaration's amendment completed. Backfill the declaration link on any certificate that still has none, then
    // reconcile exactly those freshly-linked certificates against the amended declaration.
    private async Task<List<CertificateOfOriginExceptionDto>> ExportDeclarationAmendmentSuccess(UpdateCertificateOfOriginsRequestDto request)
    {
        if (request.CertificateOfOriginsIds == null || request.CertificateOfOriginsIds.Count == 0)
        {
            return [];
        }

        var userId = RequestMetadata.UserId ?? 0;
        var certificates = await DataLayer.GetCertificatesByIds(request.CertificateOfOriginsIds);
        var backfilledIds = new List<int>();

        foreach (var certificateId in certificates.Where(certificate => string.IsNullOrEmpty(certificate.ExportDeclarationNumber)).Select(certificate => certificate.Id))
        {
            await DataLayer.UpdateCertificateDeclarationLink(certificateId, request.LeadDocumentId, request.ExportDeclarationNum, userId);
            backfilledIds.Add(certificateId);
        }

        if (backfilledIds.Count == 0)
        {
            return [];
        }

        request.CertificateOfOriginsIds = backfilledIds;
        return await ReconcileCertificatesAgainstDeclaration(request);
    }

    // ExportDeclarationReleased + AssemblySharedReleaseAccepted cases (legacy DeclarationReleased): the export
    // declaration was released. Per certificate: backfill the declaration link (recording the freshly-linked ids); a
    // PendingRelease certificate becomes Published — persist the status with the link, generate + upload its QR, and
    // publish its attachments; a certificate-replacement request cancels the replaced certificate. Finally reconcile the
    // freshly-linked certificates against the declaration.
    // TODO(blocking): the legacy release-publish also sent the request-feedback message (2281) with the rendered certificate PDF.
    // It is the same blocker as SaveCertificateOfOrigin TODO(blocking) #1 (parity B-L6): the outgoing EAI channel that carried the
    // PDF was removed from the platform. Do not send it without the PDF; when the channel returns, send it from PublishAttachments
    // so the save and the release paths get it together.
    private async Task<List<CertificateOfOriginExceptionDto>> DeclarationReleased(UpdateCertificateOfOriginsRequestDto request)
    {
        if (request.CertificateOfOriginsIds == null || request.CertificateOfOriginsIds.Count == 0)
        {
            return [];
        }

        var userId = RequestMetadata.UserId ?? 0;
        var eventUtil = Resolve<IEventUtil>();
        var certificates = await DataLayer.GetCertificatesByIds(request.CertificateOfOriginsIds);
        var backfilledIds = new List<int>();

        foreach (var certificate in certificates)
        {
            // Backfill the declaration link: take number + lead document when the certificate has none (recording the id
            // so only the freshly-linked certificates are reconciled afterwards); otherwise take just the lead document
            // when the number already matches the released declaration.
            var linkChanged = false;
            if (string.IsNullOrEmpty(certificate.ExportDeclarationNumber))
            {
                certificate.ExportDeclarationNumber = request.ExportDeclarationNum;
                certificate.LeadDocumentId = request.LeadDocumentId;
                backfilledIds.Add(certificate.Id);
                linkChanged = true;
            }
            else if (!certificate.LeadDocumentId.HasValue && certificate.ExportDeclarationNumber == request.ExportDeclarationNum)
            {
                certificate.LeadDocumentId = request.LeadDocumentId;
                linkChanged = true;
            }

            if (certificate.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.PendingRelease)
            {
                // Publish: persist the Published status together with the (possibly backfilled) declaration link, then
                // generate + upload the QR and publish the attachments (issue-by-worker queue or inline template).
                certificate.CertificateOfOriginStatusId = (int)ECertificateOfOriginStatus.Published;
                await DataLayer.UpdateCertificateReconciliation(certificate.Id, certificate.CertificateOfOriginStatusId, certificate.ExportDeclarationNumber, certificate.LeadDocumentId, certificate.RejectCancelReason, userId);

                var qrCodeToUpload = await CreateQrCodeIfNeeded(certificate);
                if (qrCodeToUpload is not null)
                {
                    // CreateQrCodeIfNeeded stamped Guid + QrImage on the entity expecting a main upsert to persist them.
                    // The release path has no upsert, so persist them explicitly before uploading the QR document (the
                    // Guid is embedded in the QR query URL).
                    await DataLayer.UpdateCertificateQrCode(certificate.Id, certificate.Guid, certificate.QrImage, userId);
                    await UploadQrCodeDocument(certificate, qrCodeToUpload, userId);
                }

                await PublishAttachments(certificate, eventUtil, userId, null);
            }
            else if (linkChanged)
            {
                await DataLayer.UpdateCertificateDeclarationLink(certificate.Id, certificate.LeadDocumentId, certificate.ExportDeclarationNumber, userId);
            }

            if (certificate.RequestReasonCode == (int)ERequestReason.CertificateReplacement)
            {
                await HandleCertificateReplacement(certificate, eventUtil);
            }
        }

        if (backfilledIds.Count == 0)
        {
            return [];
        }

        // Legacy passes a STRIPPED dto to the reconciliation on the release path (only declaration number + lead document
        // + the freshly-linked ids — no ExportInvoiceInfoList / exporter / destination / org-unit). With no invoice info
        // the reconciliation forces hasErrors, so the release-reconciled certificates take the Rejected branch. Reusing
        // the full request would instead run real invoice matching and reach DeclarationMatch/Mismatch with different
        // events — so strip it here, exactly like legacy (contrast ExportDeclarationAmendmentSuccess, which reuses the
        // full request by design).
        var reconcileRequest = new UpdateCertificateOfOriginsRequestDto
        {
            EventType = request.EventType,
            ExportDeclarationNum = request.ExportDeclarationNum,
            LeadDocumentId = request.LeadDocumentId,
            CertificateOfOriginsIds = backfilledIds,
        };
        return await ReconcileCertificatesAgainstDeclaration(reconcileRequest);
    }

    // CancellationRequestCommited case (legacy ExportDeclarationCancellationRequestCommited): the export declaration's
    // cancellation was committed — cancel every linked certificate (status → Cancelled with the canceled-declaration
    // reason) and raise the close-open-task event.
    // TODO(migration): the agent talk-back message (legacy servicesAdapter.SendMessageToAgent) is deferred — the same
    // deferral as HandleCertificateReplacement, pending the agent-message proxy.
    private async Task ExportDeclarationCancellationRequestCommited(UpdateCertificateOfOriginsRequestDto request)
    {
        if (request.CertificateOfOriginsIds == null || request.CertificateOfOriginsIds.Count == 0)
        {
            return;
        }

        var userId = RequestMetadata.UserId ?? 0;
        var eventUtil = Resolve<IEventUtil>();
        var certificates = await DataLayer.GetCertificatesByIds(request.CertificateOfOriginsIds);

        foreach (var certificate in certificates)
        {
            await DataLayer.CancelCertificate(certificate.Id, ErrorMessagesResources.CanceledDeclaration, userId);
            await RaiseCertificateEvent(eventUtil, (int)EEventType.ExportDeclarationConnectToCertificateOfOriginCanceled, certificate, null);
        }
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
        // Decided by the findings' InfException level (legacy :508-514), not by the level reported on the response rows.
        var hasErrors = reconciliation.HasNoExportInvoices || reconciliation.HasErrorFindings;
        var hasWarnings = !reconciliation.HasNoExportInvoices && reconciliation.HasWarningFindings;

        if (!hasErrors && !hasWarnings)
        {
            await RaiseCertificatePreferredAssessorEvent(certificate, eventUtil, (int)EEventType.CertificateOfOriginCertificateMatchDeclaration);
            return ((int)ECertificateOfOriginStatus.DeclarationMatch, certificate.RejectCancelReason);
        }

        // The concatenated exception texts carried as the event AdditionalInfo (legacy, capped).
        var additionalInfo = BuildReconciliationAdditionalInfo(reconciliation.Exceptions);

        if (hasErrors)
        {
            await RaiseCertificateEvent(eventUtil, (int)EEventType.CertificateOfOriginCertificateDeclarationMismatch, certificate, additionalInfo, CertificateOfOriginsConsts.ExportOrganizationUnitType);
            return ((int)ECertificateOfOriginStatus.Rejected, CertificateOfOriginsConsts.ReconciliationMismatchReason);
        }

        // Warnings only.
        if (reconciliation.IsLinkedToImportDeclaration)
        {
            // Legacy: an import-certificate replacement whose associated goods are not in the trade agreement opens a
            // task to handle the replacement.
            await RaiseImportReplacementTaskEvent(eventUtil, certificate);
        }

        await RaiseDeclarationHasWarningsEvent(certificate, request, eventUtil, additionalInfo);
        return ((int)ECertificateOfOriginStatus.DeclarationMismatch, certificate.RejectCancelReason);
    }

    // Resolve the export-declaration assessor handling the lead document (legacy
    // GetLatestUserHandlingEntityTasksWithTaskUnification → the resolved user id), or null when there is no lead
    // document / no handling user.
    private async Task<int?> ResolveAssessorUserId(int? leadDocumentId, int organizationUnitId)
    {
        var tasksProxy = Resolve<ITasksProxy>();
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
    //
    // CR 194221: the event's AdditionalInfo lands in {2} of the task-type title pattern, so it is what renders the
    // request reason in the task name "טיפול בבקשה להנפקת תעודת מקור - (סיבת בקשה)" (Tasks_enum_TaskType 181,
    // PC_Tsk01_NewCertificateOfOriginCheck). Both events wired to open that task (599 NewCertificateOfOriginCreated
    // and 642 CertificateMatchDeclaration) ran through this same legacy method, so placing the value here covers
    // both. 642 is the only one that actually fires: legacy RaiseNewCertificateOfOriginCreatedEvent raises 599 only
    // when the "IsExportDeclarationActive" parameter is FALSE, and that parameter is always TRUE (confirmed with the
    // analyst, 2026-09-23) — so 599 is unreachable in the legacy too, not just here. See RaiseStatusEvents.
    //
    // The AdditionalInfo follows the legacy as updated with CR 194221 (B-L9, 2026-10-11): the request-reason name,
    // followed by ". " and the "additional certificates on this declaration" message when the declaration carries other
    // live certificates.
    private async Task RaiseCertificatePreferredAssessorEvent(CertificateOfOrigin certificate, IEventUtil eventUtil, int eventTypeId)
    {
        var assessorUserId = await ResolveAssessorUserId(certificate.LeadDocumentId, certificate.OrganizationUnitId);
        var builder = CreateCertificateEventBuilder(eventUtil, eventTypeId, certificate);

        // Legacy RaiseTaskNewCertificateOfOriginCheck set the organization-unit TYPE (Export) on the event.
        builder = builder.WithOrganizationUnitTypeId((CustomsCloud.InfrastructureCore.Interfaces.Shared.OrganizationUnitTypes)CertificateOfOriginsConsts.ExportOrganizationUnitType);

        if (assessorUserId.HasValue)
        {
            builder = builder.WithTaskArguments(task => task.WithPreferredUserId(assessorUserId.Value));
        }

        // CR 194221 — rendered into {2} of the task title.
        var additionalInfo = await BuildNewCertificateCheckAdditionalInfo(certificate);
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            builder = builder.WithAdditionalInfo(additionalInfo);
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy RaiseTaskNewCertificateOfOriginCheck (CR 194221): with other live certificates on the declaration the text is the
    // reason name, a period and a space, then AdditionalCertificatesToExportDeclaration filled with one number, two numbers, or
    // the words לדוגמה with the first and the last number. With none it is the reason name alone.
    private async Task<string?> BuildNewCertificateCheckAdditionalInfo(CertificateOfOrigin certificate)
    {
        var requestReasonName = GetRequestReasonName(certificate.RequestReasonCode);
        var certificateNumbers = await GetOtherCertificateNumbersOnDeclaration(certificate);
        var result = FormatNewCertificateCheckAdditionalInfo(requestReasonName, certificateNumbers);
        return result;
    }

    private static string FormatNewCertificateCheckAdditionalInfo(string requestReasonName, List<string> certificateNumbers)
    {
        if (certificateNumbers.Count == 0)
        {
            return requestReasonName;
        }

        var messageParameter = certificateNumbers.Count switch
        {
            1 => certificateNumbers[0],
            2 => certificateNumbers[0] + ", " + certificateNumbers[1],
            _ => ". לדוגמה:" + certificateNumbers[0] + ", " + certificateNumbers[^1],
        };
        var message = string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.AdditionalCertificatesToExportDeclaration, messageParameter);
        var result = requestReasonName + ". " + message;
        return result;
    }

    // Legacy GetCertificateOfOriginByDeclaration + the caller's filter: the certificates on the export declaration's goods
    // items, not Cancelled / Rejected / Error, other than this one's number. No declaration, or no declaration info, gives
    // none (the legacy `?.` of CR 194221). A null number is skipped: legacy's cn.Equals would have thrown on it.
    // TODO(confirm live): B-L9 is covered by unit tests only — run a reconcile match against a real ExportDealFile service
    // with a declaration that carries other certificates, and check the task name. No real ExportDealFile is reachable yet.
    private async Task<List<string>> GetOtherCertificateNumbersOnDeclaration(CertificateOfOrigin certificate)
    {
        if (certificate.LeadDocumentId == null && certificate.ExportDeclarationNumber == null)
        {
            return [];
        }

        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(certificate.LeadDocumentId, certificate.ExportDeclarationNumber);
        if (declarationDetails == null)
        {
            return [];
        }

        var declarationInfo = await exportDealFileProxy.GetExportDeclarationInfoForPc(declarationDetails.LeadDocumentId);
        if (declarationInfo == null)
        {
            return [];
        }

        var certificateIds = declarationInfo.ExportInvoiceInfoList
            .SelectMany(invoice => invoice.ExportGoodsItemInfoList ?? [])
            .Select(goodsItem => goodsItem.CertificateOfOriginId)
            .ToList();
        var certificateNumbers = await DataLayer.GetLiveCertificateNumbersByIds(certificateIds);
        var result = certificateNumbers
            .Where(number => number != null && !number.Equals(certificate.CertificateNumber, StringComparison.Ordinal))
            .Select(number => number!)
            .ToList();
        return result;
    }

    // The request-reason display name — taken from the ERequestReason [Display(Name)] attribute (the Hebrew names of
    // CRM.CertificateOfOrigins_enum_RequestReasonCode), mirroring GetCertificateTypeName. An unmapped code yields an
    // empty string rather than a bare number, so the task name degrades to the plain title instead of showing "(17)".
    private static string GetRequestReasonName(int requestReasonCode)
    {
        var result = GetEnumDisplayName<ERequestReason>(requestReasonCode);
        return result;
    }

    // The status display name — the Hebrew `Name` of CRM.CertificateOfOrigins_enum_CertificateOfOriginStatusCode, as legacy
    // put the table Name into a message parameter.
    private static string GetCertificateStatusName(int statusId)
    {
        var result = GetEnumDisplayName<ECertificateOfOriginStatus>(statusId);
        return result;
    }

    private static string GetEnumDisplayName<TEnum>(int value) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            return string.Empty;
        }

        var memberName = Enum.GetName(typeof(TEnum), value)!;
        var member = typeof(TEnum).GetMember(memberName).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? memberName;
    }

    // Legacy warnings branch: raise CertificateOfOriginCertificateDeclarationHasWarnings, assigning the mismatch task to
    // the export-declaration assessor (the resolved user id; the assessor OrganizationUnitId source here is the DTO's,
    // per the legacy). The task is assigned only when an assessor is found.
    private async Task RaiseDeclarationHasWarningsEvent(CertificateOfOrigin certificate, UpdateCertificateOfOriginsRequestDto request, IEventUtil eventUtil, string additionalInfo)
    {
        var assessorUserId = await ResolveAssessorUserId(certificate.LeadDocumentId, request.OrganizationUnitId);
        var builder = CreateCertificateEventBuilder(eventUtil, (int)EEventType.CertificateOfOriginCertificateDeclarationHasWarnings, certificate);

        // Legacy set the organization-unit TYPE (Export) on the warnings event's VirtualEntity.
        builder = builder.WithOrganizationUnitTypeId((CustomsCloud.InfrastructureCore.Interfaces.Shared.OrganizationUnitTypes)CertificateOfOriginsConsts.ExportOrganizationUnitType);

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            builder = builder.WithAdditionalInfo(additionalInfo);
        }

        if (assessorUserId.HasValue)
        {
            // Legacy set a SingleUserAssignmentFilter with EProfession.Marech + the Export organization-unit type besides the
            // user id. The platform used only the user id when it was set, in AssignToUserBySingleUserAssignmentFilter.GetFilter,
            // and the other fields only fed the assignment log text. So assigning by the resolved assessor user id is parity.
            // The Export unit type is set on the event above.
            builder = builder.WithTaskArguments(task => task.WithTaskAssignmentUser(assessorUserId.Value));
        }

        await eventUtil.RaiseEvent(builder.Build());
    }

    // Legacy: concatenate the reconciliation exception texts into the event AdditionalInfo, capped so the task field is
    // not overflowed (MaximumNumberOfCharactersOfTheField, reserving LengthOfTaskStart).
    private static string BuildReconciliationAdditionalInfo(IEnumerable<CertificateOfOriginExceptionDto> exceptions)
    {
        var additionalInfo = new StringBuilder();
        foreach (var exception in exceptions)
        {
            var text = exception.ExceptionDescription ?? string.Empty;
            if (additionalInfo.Length + text.Length + CertificateOfOriginsConsts.LengthOfTaskStart < CertificateOfOriginsConsts.MaximumNumberOfCharactersOfTheField)
            {
                additionalInfo.Append(text).Append(" , ");
            }
        }

        return additionalInfo.ToString();
    }

    // Legacy ValidateExportDeclarationInfoForPCIsMatch: compares the certificate against the export declaration and
    // collects the mismatch exceptions (deduped by message, like legacy PassGroupdExceptionListToEntity's GroupBy).
    // The finding levels are the legacy InfException levels, which decide the outcome (a null level counts as an error; only
    // the import-replacement and unmatched-invoice checks are warnings); the response rows carry the UIMessage row instead
    // (ReconciliationUiMessages). Returns the exceptions + whether the certificate is now linked to an import declaration.
    // isLeadDocumentTitleMismatch: the save's title check failed (legacy: already in _requestExceptions when this ran). It is
    // the first finding, so it survives every early return below and wins the de-duplication against the goods-item check
    // that reports under the same message.
    private async Task<ReconciliationResult> ValidateReconciliation(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateOfOriginDetails> details,
        bool isLeadDocumentTitleMismatch = false)
    {
        var builder = new List<ReconciliationFinding>();
        if (isLeadDocumentTitleMismatch)
        {
            builder.Add(CertificateNumberNotInDealFileFinding());
        }

        // No invoices in the declaration → Rejected. Legacy set hasErrors = true with no exception of its own: no error row,
        // no AdditionalInfo on the mismatch event, nothing returned to the caller — beyond a title mismatch already found.
        if (request.ExportInvoiceInfoList == null || request.ExportInvoiceInfoList.Count == 0)
        {
            return BuildReconciliationResult(builder, false, hasNoExportInvoices: true);
        }

        // A certificate with no invoice rows is a match, whatever its own details say. Faithful to production (analyst
        // decision, 2026-09-28): legacy ValidateExportDeclarationInfoForPCIsMatch collected the detail and
        // import-replacement findings in a local list and handed them to the result (PassGroupdExceptionListToEntity)
        // only inside its invoice block, so without invoice rows they were dropped. IsLinkedToImportDeclaration is read
        // only on the warnings path, which a finding-free result never reaches.
        var invoices = await DataLayer.GetCertificateInvoiceDetailsByCertificateIds([certificate.Id]);
        if (invoices.Count == 0)
        {
            return BuildReconciliationResult(builder, false);
        }

        await ValidateCertificateDetails(request, certificate, details, builder);
        var isLinkedToImportDeclaration = await ValidateImportReplacement(request, certificate, builder);

        // Invoice / goods-item / customs-item matching (the declaration already carries invoices — checked above).
        var originCountry = GetDetailValue(details, ECertificateDetailsType.OriginCountry);
        var originGroup = GetDetailValue(details, ECertificateDetailsType.OriginGroupOfCountries);
        await ValidateInvoiceMatching(request.ExportInvoiceInfoList, certificate, invoices, originCountry, originGroup, builder);

        return BuildReconciliationResult(builder, isLinkedToImportDeclaration);
    }

    // Scalar/detail checks: destination country + destination country-group, export-declaration number, exporter
    // (all Error level, since the legacy raised them with no explicit level).
    private async Task ValidateCertificateDetails(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<CertificateOfOriginDetails> details,
        List<ReconciliationFinding> builder)
    {
        var destinationCountry = GetDetailValue(details, ECertificateDetailsType.DestinationCountry);
        var destinationGroup = GetDetailValue(details, ECertificateDetailsType.DestinationGroupOfCountries);
        var exporterId = GetDetailValue(details, ECertificateDetailsType.ExporterId);

        // Destination country on the certificate must match the declaration's destination (Error).
        if (request.DestinationCountryId.HasValue
            && int.TryParse(destinationCountry, out var destinationCountryId)
            && destinationCountryId != request.DestinationCountryId.Value)
        {
            builder.Add(new ReconciliationFinding(
                EReconciliationMessage.DestinationCountryMismatch,
                (int)EExceptionLevel.Error,
                ErrorMessagesResources.DestinationCountryMismatch));
        }

        // Destination country-group agreement — the buyer country must belong to the certificate's destination group
        // (Error). Legacy looks up the (destination country, group) pair; a null declaration destination country cannot
        // match the pair, so it is a discrepancy — the check is NOT skipped on a null destination country.
        if (!string.IsNullOrWhiteSpace(destinationGroup) && int.TryParse(destinationGroup, out var destinationGroupId))
        {
            var destinationInGroup = request.DestinationCountryId.HasValue
                && await IsCountryInCountryGroup(request.DestinationCountryId.Value, destinationGroupId);
            if (!destinationInGroup)
            {
                builder.Add(new ReconciliationFinding(
                    EReconciliationMessage.DestinationGroupDiscrepancy,
                    (int)EExceptionLevel.Error,
                    ErrorMessagesResources.DestinationGroupDiscrepancy));
            }
        }

        // The export-declaration number already stamped on the certificate must match the declaration's (legacy
        // ExportDeclarationNotInSystemForWarningMessage — raised with no explicit level, so null / Error per legacy).
        if (!string.IsNullOrEmpty(certificate.ExportDeclarationNumber)
            && certificate.ExportDeclarationNumber != request.ExportDeclarationNum)
        {
            builder.Add(new ReconciliationFinding(
                EReconciliationMessage.ExportDeclarationNotInSystem,
                (int)EExceptionLevel.Error,
                string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.ExportDeclarationNotInSystem, request.ExportDeclarationNum)));
        }

        // The exporter on the certificate must match the declaration's exporter (Error; the legacy compares int to
        // nullable int, so a null declaration exporter also flags a mismatch — preserved).
        if (int.TryParse(exporterId, out var exporter) && exporter != request.ExporterCustomerId)
        {
            builder.Add(new ReconciliationFinding(
                EReconciliationMessage.ExporterMismatch,
                (int)EExceptionLevel.Error,
                ErrorMessagesResources.ExporterMismatch));
        }
    }

    // Import-certificate replacement: at least one associated import goods item's origin country must be party to the
    // certificate type's trade agreement, else a warning + the certificate is linked to an import declaration (returns
    // isLinkedToImportDeclaration).
    private async Task<bool> ValidateImportReplacement(
        UpdateCertificateOfOriginsRequestDto request,
        CertificateOfOrigin certificate,
        List<ReconciliationFinding> builder)
    {
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        if (certificate.RequestReasonCode != (int)ERequestReason.ImportCertificateReplacement)
        {
            return false;
        }

        var associatedGoodsItems = await exportDealFileProxy.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(request.LeadDocumentId);
        var isInTradeAgreement = false;
        foreach (var goodsItem in associatedGoodsItems ?? [])
        {
            if (await IsTradeAgreementForCountry(certificate.TypeId, goodsItem.AssociatedOriginCountryId, false))
            {
                isInTradeAgreement = true;
                break;
            }
        }

        if (associatedGoodsItems is { Count: > 0 } && isInTradeAgreement)
        {
            return false;
        }

        builder.Add(new ReconciliationFinding(
            EReconciliationMessage.ImportReplacementNoTradeAgreement,
            (int)EExceptionLevel.Warning,
            ErrorMessagesResources.ImportReplacementNoTradeAgreement));
        return true;
    }

    // Legacy ValidateExportDeclarationInfoForPCIsMatch invoice block (item.CertificateOfOriginInvoiceDetail loop): each
    // certificate invoice must match a declaration invoice by number, and — when the certificate type requires it — the
    // goods items' origin country / country-group / certificate link / 6-digit customs classification must match, in
    // both directions.
    private async Task ValidateInvoiceMatching(
        List<ExportInvoiceInfoDto> declarationInvoices,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        string? originCountry,
        string? originGroup,
        List<ReconciliationFinding> builder)
    {
        var customsBookProxy = Resolve<ICustomsBookProxy>();
        var isCustomsItemMandatory = await DataLayer.GetCertificateTypeIsCustomsItemMandatory(certificate.TypeId) ?? false;

        // Resolve the 6-digit tariff classification of every customs item on both sides in one batch.
        var certificateCustomsItemIds = invoices
            .SelectMany(invoice => invoice.GoodsItemCustomsItemIds)
            .Where(customsItemId => customsItemId.HasValue)
            .Select(customsItemId => customsItemId!.Value);
        var declarationCustomsItemIds = declarationInvoices
            .SelectMany(invoice => invoice.ExportGoodsItemInfoList ?? [])
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
        var allInvoicesMatched = await ValidateCertificateInvoices(declarationInvoices, certificate, invoices, originCountry, originGroup, sixDigitByCustomsItemId, isCustomsItemMandatory, builder);

        // Reverse: every declaration goods item linked to this certificate must have its 6-digit classification present
        // in the certificate's matching invoice (Error) — only when the type requires it and all invoices matched.
        if (allInvoicesMatched && isCustomsItemMandatory)
        {
            ValidateDeclarationGoodsItems(declarationInvoices, certificate, invoices, sixDigitByCustomsItemId, builder);
        }
    }

    // Forward direction: each certificate invoice must match a declaration invoice by number (else a warning + stop),
    // then each of its goods items is validated. Returns false if an invoice did not match (legacy returned from the
    // whole validator on the first unmatched invoice).
    private async Task<bool> ValidateCertificateInvoices(
        List<ExportInvoiceInfoDto> declarationInvoices,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        string? originCountry,
        string? originGroup,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        bool isCustomsItemMandatory,
        List<ReconciliationFinding> builder)
    {
        foreach (var invoice in invoices)
        {
            var declarationInvoice = declarationInvoices.FirstOrDefault(declaration => declaration.ExternalIdNum == invoice.InvoiceNumber);
            if (declarationInvoice == null)
            {
                builder.Add(new ReconciliationFinding(
                    EReconciliationMessage.ExportInvoiceNotMatch,
                    (int)EExceptionLevel.Warning,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.ExportInvoiceNotMatch, invoice.InvoiceNumber)));
                return false;
            }

            // Legacy runs the per-goods-item checks only when BOTH sides are non-empty; the certificate side is implicit
            // (the loop below is empty when it has no customs items). Skip when the matched declaration invoice carries
            // no goods items — otherwise the absence-based checks (!Any) would false-positive on the empty list.
            var declarationGoodsItems = declarationInvoice.ExportGoodsItemInfoList;
            if (declarationGoodsItems == null || declarationGoodsItems.Count == 0)
            {
                continue;
            }

            var declarationCustomsItemIdsForInvoice = declarationGoodsItems
                .Where(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id)
                .Select(goodsItem => goodsItem.CustomsItemId)
                .ToList();

            // Every goods item, with or without a customs item (legacy CertificateOfOriginsBL.cs:809, parity finding BN-01):
            // the origin-country, country-group and deal-file checks do not depend on the customs item.
            foreach (var certificateCustomsItemId in invoice.GoodsItemCustomsItemIds)
            {
                await ValidateCertificateGoodsItem(certificate, declarationGoodsItems, invoice.InvoiceNumber, certificateCustomsItemId, declarationCustomsItemIdsForInvoice, originCountry, originGroup, sixDigitByCustomsItemId, isCustomsItemMandatory, builder);
            }
        }

        return true;
    }

    // One certificate goods item vs the matched declaration invoice: origin country, origin country-group, certificate
    // link, and the forward customs-item 6-digit match.
    private async Task ValidateCertificateGoodsItem(
        CertificateOfOrigin certificate,
        List<ExportGoodsItemInfoDto> declarationGoodsItems,
        string? invoiceNumber,
        int? certificateCustomsItemId,
        List<int> declarationCustomsItemIdsForInvoice,
        string? originCountry,
        string? originGroup,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        bool isCustomsItemMandatory,
        List<ReconciliationFinding> builder)
    {
        // Origin country present among the declaration's goods items (Error).
        if (int.TryParse(originCountry, out var originCountryId)
            && !declarationGoodsItems.Any(goodsItem => goodsItem.OriginCountryId == originCountryId))
        {
            builder.Add(new ReconciliationFinding(
                EReconciliationMessage.OriginCountryMismatch,
                (int)EExceptionLevel.Error,
                ErrorMessagesResources.OriginCountryMismatch));
        }

        // Origin country-group agreement — any declaration origin country must be in the group (Error).
        if (!string.IsNullOrWhiteSpace(originGroup) && int.TryParse(originGroup, out var originGroupId))
        {
            var anyOriginInGroup = false;
            foreach (var goodsItem in declarationGoodsItems)
            {
                if (await IsCountryInCountryGroup(goodsItem.OriginCountryId, originGroupId))
                {
                    anyOriginInGroup = true;
                    break;
                }
            }

            if (!anyOriginInGroup)
            {
                builder.Add(new ReconciliationFinding(
                    EReconciliationMessage.OriginCountryMismatch,
                    (int)EExceptionLevel.Error,
                    ErrorMessagesResources.OriginCountryMismatch));
            }
        }

        // The certificate must be linked to at least one declaration goods item (Error).
        if (!declarationGoodsItems.Any(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id))
        {
            builder.Add(CertificateNumberNotInDealFileFinding());
        }

        // Customs-item 6-digit classification match, forward (Error) — only when the type requires it. A goods item without
        // a customs item has no 6 digits, so legacy skipped it here (!customsItemBy6Digits.IsNullOrEmpty()).
        if (isCustomsItemMandatory
            && certificateCustomsItemId is int customsItemId
            && sixDigitByCustomsItemId.TryGetValue(customsItemId, out var certificateSixDigits)
            && !string.IsNullOrEmpty(certificateSixDigits)
            && !declarationCustomsItemIdsForInvoice.Any(declarationCustomsItemId =>
                sixDigitByCustomsItemId.TryGetValue(declarationCustomsItemId, out var declarationSixDigits)
                && !string.IsNullOrEmpty(declarationSixDigits)
                && declarationSixDigits == certificateSixDigits))
        {
            builder.Add(new ReconciliationFinding(
                EReconciliationMessage.CustomsItemMismatch,
                (int)EExceptionLevel.Error,
                string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.CustomsItemMismatch, certificateSixDigits, invoiceNumber)));
        }
    }

    // Reverse direction: every declaration goods item linked to this certificate must have its 6-digit classification
    // present in the certificate's matching invoice (Error).
    // Legacy SystemTablesUtil.GetTablesSync<CountryCountryGroup>(IgnoreState = true, CountryID == x && CountryGroupID == y):
    // the country belongs to the country group (no state filter, no date window — as legacy).
    private async Task<bool> IsCountryInCountryGroup(int countryId, int countryGroupId)
    {
        var result = (await lookupUtil.Search<Lookups.CountryCountryGroup>(link => link.CountryId == countryId && link.CountryGroupId == countryGroupId)).Any();
        return result;
    }

    private static void ValidateDeclarationGoodsItems(
        List<ExportInvoiceInfoDto> declarationInvoices,
        CertificateOfOrigin certificate,
        List<CertificateReconcileInvoiceDto> invoices,
        Dictionary<int, string?> sixDigitByCustomsItemId,
        List<ReconciliationFinding> builder)
    {
        foreach (var declarationInvoice in declarationInvoices)
        {
            if (declarationInvoice.ExportGoodsItemInfoList == null)
            {
                continue;
            }

            var certificateCustomsItemIdsForInvoice = invoices
                .FirstOrDefault(invoice => invoice.InvoiceNumber == declarationInvoice.ExternalIdNum)?.GoodsItemCustomsItemIds ?? [];
            foreach (var goodsItem in declarationInvoice.ExportGoodsItemInfoList.Where(goodsItem => goodsItem.CertificateOfOriginId == certificate.Id))
            {
                if (sixDigitByCustomsItemId.TryGetValue(goodsItem.CustomsItemId, out var declarationSixDigits)
                    && !string.IsNullOrEmpty(declarationSixDigits)
                    && !certificateCustomsItemIdsForInvoice.Any(certificateCustomsItemId =>
                        certificateCustomsItemId is int customsItemId
                        && sixDigitByCustomsItemId.TryGetValue(customsItemId, out var certificateSixDigits)
                        && !string.IsNullOrEmpty(certificateSixDigits)
                        && certificateSixDigits == declarationSixDigits))
                {
                    builder.Add(new ReconciliationFinding(
                        EReconciliationMessage.CustomsItemInDeclarationNotInCertificate,
                        (int)EExceptionLevel.Error,
                        string.Format(CultureInfo.InvariantCulture, ErrorMessagesResources.CustomsItemInDeclarationNotInCertificate, declarationSixDigits, declarationInvoice.ExternalIdNum)));
                }
            }
        }
    }

    // EMessages.CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile — raised by the goods-item link check and by the
    // save's lead-document title check.
    private static ReconciliationFinding CertificateNumberNotInDealFileFinding()
    {
        return new ReconciliationFinding(
            EReconciliationMessage.CertificateNumberNotInDealFile,
            (int)EExceptionLevel.Error,
            ErrorMessagesResources.CertificateNumberNotInDealFile);
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
    // Two different levels, as in legacy: the outcome (Rejected / DeclarationMismatch) is decided by the InfException level of
    // the findings (CertificateOfOriginsBL.cs:513-514, no level = Error), while the response row carries the UIMessage row's
    // UIMessageTypeID and EnglishName (GetUIMessageWithEnglishAndLevel, :538, :558-559).
    private static ReconciliationResult BuildReconciliationResult(List<ReconciliationFinding> builder, bool isLinkedToImportDeclaration, bool hasNoExportInvoices = false)
    {
        var findings = builder
            .GroupBy(finding => finding.Key)
            .Select(group => group.First())
            .ToList();
        var exceptions = findings
            .Select(finding => new CertificateOfOriginExceptionDto
            {
                ExceptionLevel = ReconciliationUiMessages[finding.Key].Level,
                ExceptionDescription = finding.Description,
                EnglishDescription = ReconciliationUiMessages[finding.Key].EnglishName,
                ExceptionType = (int)finding.Key,
            })
            .ToList();
        return new ReconciliationResult(
            exceptions,
            isLinkedToImportDeclaration,
            hasNoExportInvoices,
            findings.Exists(finding => finding.Level == (int)EExceptionLevel.Error),
            findings.Exists(finding => finding.Level == (int)EExceptionLevel.Warning));
    }

    // The legacy UIMessage row of each reconciliation message (Shared.General_enum_UIMessage, queried 2026-10-15): EnglishName
    // (the enum name, not a translation) and UIMessageTypeID (1 = Error, 2 = Warning, the EExceptionLevel values). Three of them
    // differ from the decision level: DestinationGroupDiscrepancy and CustomsItemInDeclarationNotInCertificate decide as errors but
    // report 2, ExportInvoiceNotMatch decides as a warning but reports 1.
    private static readonly IReadOnlyDictionary<EReconciliationMessage, (string EnglishName, int Level)> ReconciliationUiMessages =
        new Dictionary<EReconciliationMessage, (string EnglishName, int Level)>
        {
            [EReconciliationMessage.OriginCountryMismatch] = ("OriginCountryIsNotMAtchToOriginCountryInExportdeclaration", 1),
            [EReconciliationMessage.DestinationCountryMismatch] = ("DestinationCountryIsNotMAtchTofDestinationCountryInExportdeclaration", 1),
            [EReconciliationMessage.CertificateNumberNotInDealFile] = ("CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile", 1),
            [EReconciliationMessage.ExporterMismatch] = ("ExporterNumberIsNotMAtchToExporterNumberInExportdeclaration", 1),
            [EReconciliationMessage.ExportInvoiceNotMatch] = ("ExportInvoiceIsNotMAtchToExportInvoiceInExportdeclaration", 1),
            [EReconciliationMessage.CustomsItemMismatch] = ("CustomsItemIsNotMAtchToCustomsItemInExportdeclaration", 1),
            [EReconciliationMessage.ExportDeclarationNotInSystem] = ("ExportDeclarationNotInSystemForWarningMessage", 1),
            [EReconciliationMessage.DestinationGroupDiscrepancy] = ("DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer", 2),
            [EReconciliationMessage.ImportReplacementNoTradeAgreement] = ("ThereIsNoMerchandiseInTheDeclarationThatIsLinkedToTheImportDeclarationWhoseCountryOfOriginIsInTheTradeAgreement", 2),
            [EReconciliationMessage.CustomsItemInDeclarationNotInCertificate] = ("CustomsItemInDeclarationIsNotMAtchToCustomsItemsInCertificate", 2),
        };

    // Dedup key for the reconciliation exceptions — reproduces the legacy GroupBy(InfException.UserMessage). Each member's
    // NUMERIC VALUE is the legacy EMessages id (noted beside it), carried as ExceptionType; its Hebrew text is the
    // ErrorMessagesResources entry of the same name, and its English name and response level are in ReconciliationUiMessages.
    private enum EReconciliationMessage
    {
        DestinationCountryMismatch = 1402655,                   // EMessages.DestinationCountryIsNotMAtchTofDestinationCountryInExportdeclaration
        DestinationGroupDiscrepancy = 14027170,                 // EMessages.DiscrepancyBetweenTheCountriesInTheAgreementVersusTheCountryOfTheBuyer
        ExportDeclarationNotInSystem = 14026636,                // EMessages.ExportDeclarationNotInSystemForWarningMessage
        ExporterMismatch = 14026543,                            // EMessages.ExporterNumberIsNotMAtchToExporterNumberInExportdeclaration
        ImportReplacementNoTradeAgreement = 14027187,           // EMessages.ThereIsNoMerchandiseInTheDeclarationThatIsLinkedToTheImportDeclarationWhoseCountryOfOriginIsInTheTradeAgreement
        OriginCountryMismatch = 1402654,                        // EMessages.OriginCountryIsNotMAtchToOriginCountryInExportdeclaration
        CertificateNumberNotInDealFile = 14026542,              // EMessages.CertificateNumberISNotMatchToCerrtificateNumberInExportDealFile
        CustomsItemMismatch = 14026547,                         // EMessages.CustomsItemIsNotMAtchToCustomsItemInExportdeclaration
        ExportInvoiceNotMatch = 14026546,                       // EMessages.ExportInvoiceIsNotMAtchToExportInvoiceInExportdeclaration
        CustomsItemInDeclarationNotInCertificate = 14027278,    // EMessages.CustomsItemInDeclarationIsNotMAtchToCustomsItemsInCertificate
    }

    // level: the legacy InfException level, which decides the outcome (not the reported level — see ReconciliationUiMessages).
    private sealed class ReconciliationFinding(EReconciliationMessage key, int level, string description)
    {
        public EReconciliationMessage Key { get; } = key;

        public int Level { get; } = level;

        public string Description { get; } = description;
    }

    private sealed class ReconciliationResult(
        List<CertificateOfOriginExceptionDto> exceptions,
        bool isLinkedToImportDeclaration,
        bool hasNoExportInvoices,
        bool hasErrorFindings,
        bool hasWarningFindings)
    {
        public List<CertificateOfOriginExceptionDto> Exceptions { get; } = exceptions;

        public bool IsLinkedToImportDeclaration { get; } = isLinkedToImportDeclaration;

        // The declaration carried no invoices: an error outcome (Rejected) that legacy raised with no exception.
        public bool HasNoExportInvoices { get; } = hasNoExportInvoices;

        // The findings by their decision (InfException) level.
        public bool HasErrorFindings { get; } = hasErrorFindings;

        public bool HasWarningFindings { get; } = hasWarningFindings;
    }
}
