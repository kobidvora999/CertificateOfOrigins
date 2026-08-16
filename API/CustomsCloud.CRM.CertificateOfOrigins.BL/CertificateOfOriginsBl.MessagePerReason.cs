using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// GetPC_MSG2280_2281 create branch — the per-reason resolution (legacy CheckRequestReasonAndGetSavedCertificate + its
// helpers). Runs before the save: resolves the existing certificate the reason targets, runs the reason-specific
// validations (accumulated in-band on the context, never thrown), and records the side-values the save consumes
// (CertificateToUpdateId / CertificateIdToCancel). The ambient _certificateToUpdateId etc. become explicit context fields.
public partial class CertificateOfOriginsBl
{
    // Legacy CheckRequestReasonAndGetSavedCertificate: the switch over ERequestReason. Returns the existing certificate
    // the request refers to (null when none / not applicable). Exporter id is already resolved on the context (from the
    // field validation) — CheckCertificateUpdate reads it for the agent-mismatch check.
    private async Task<CertificateOfOrigin?> ResolveCertificateForReason(CertificateOfOriginAgentRequestDto agentRequest, MessageValidationContext context)
    {
        var reason = agentRequest.RequestReasonCode;
        CertificateOfOrigin? certificate = null;

        switch (reason)
        {
            case (int)ERequestReason.EmptyCertificate:
                CheckCertificateIdIsEmpty(agentRequest.CertificateId, context);
                break;

            case (int)ERequestReason.CertificateCancellation:
                certificate = await CheckCertificateNumber(agentRequest.CertificateId, context);
                if (certificate != null)
                {
                    await CheckDeclarationAssociatedWithCertificate(certificate.Id, context);
                }

                break;

            case (int)ERequestReason.CertificateUpdate:
                certificate = await CheckCertificateNumber(agentRequest.CertificateId, context);
                if (certificate != null)
                {
                    CheckCertificateUpdate(agentRequest, certificate, context);
                }

                break;

            case (int)ERequestReason.CertificateReplacement:
                CheckCertificateIdToCancelHasValue(agentRequest.CertificateIdToCancel, context);
                certificate = await CheckCertificateNumber(agentRequest.CertificateIdToCancel, context);
                if (!string.IsNullOrEmpty(agentRequest.CertificateId))
                {
                    // Legacy: the supplied replacement certificateID (distinct from certificateIdToCancel) must exist
                    // AND not already be Published/Cancelled.
                    await CheckCertificateNumber(agentRequest.CertificateId, context);
                    await CheckIfCertificatePublishedOrCanceled(agentRequest.CertificateId, context);
                }

                if (certificate != null)
                {
                    ValidateCertificateInReplacement(agentRequest, certificate, context);
                    context.CertificateIdToCancel = certificate.Id;
                }

                await CheckExportDeclarationNumber(agentRequest.ExportDeclarationNum, context);
                break;

            case (int)ERequestReason.ImportCertificateReplacement:
                CheckCertificateIdToCancelHasValue(agentRequest.CertificateIdToCancel, context);
                break;

            case (int)ERequestReason.RetrospectiveCertificate:
                await CheckExportDeclarationNumber(agentRequest.ExportDeclarationNum, context);
                if (!string.IsNullOrEmpty(agentRequest.CertificateId))
                {
                    await CheckIfCertificatePublishedOrCanceled(agentRequest.CertificateId, context);
                }

                break;

            case (int)ERequestReason.NewCertificate:
                if (!string.IsNullOrEmpty(agentRequest.CertificateId))
                {
                    certificate = await CheckCertificateNumber(agentRequest.CertificateId, context);
                    await CheckIfCertificatePublishedOrCanceled(agentRequest.CertificateId, context);
                }

                break;

            case (int)ERequestReason.Draft:
                if (!string.IsNullOrEmpty(agentRequest.CertificateId))
                {
                    await CheckIfCertificatePublishedOrCanceled(agentRequest.CertificateId, context);
                }

                break;

            default:
                break;
        }

        // Legacy tail: re-fetch if still unresolved, then block a correction while a customs-employee task exists
        // (DeclarationMatch / DeclarationMismatch status).
        certificate ??= await GetExistingCertificate(agentRequest.CertificateId);
        if (certificate != null
            && (certificate.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.DeclarationMatch
                || certificate.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatus.DeclarationMismatch))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ItIsNotPossibleToTransmitACorrectionWhenThereIsATaskForACustomsEmployee, certificate.CertificateNumber));
        }

        return certificate;
    }

    // Legacy CheckCertificateNumber: resolve the existing certificate by number; missing id / not-found are in-band errors.
    private async Task<CertificateOfOrigin?> CheckCertificateNumber(string? certificateNumber, MessageValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(certificateNumber))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.MustSendCertificateID));
            return null;
        }

        var certificate = await GetExistingCertificate(certificateNumber);
        if (certificate == null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CertificateDoesntExist, certificateNumber));
        }

        return certificate;
    }

    // Legacy CheckCertificateUpdate: agent (exporter) must match, certificate type must not switch NonManipulation
    // on/off, and a Published/Cancelled certificate cannot be updated. Records the update-target id (legacy
    // _certificateToUpdateId, set unconditionally — even when a validation error was raised).
    private static void CheckCertificateUpdate(CertificateOfOriginAgentRequestDto agentRequest, CertificateOfOrigin certificate, MessageValidationContext context)
    {
        if (context.ExporterId != certificate.CustomerId)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.AgentInUpdateDifferent, certificate.CertificateNumber));
        }

        if (agentRequest.CertificateOfOriginTypeCode != certificate.TypeId
            && (agentRequest.CertificateOfOriginTypeCode == (int)ECertificateOfOriginType.NonManipulation
                || certificate.TypeId == (int)ECertificateOfOriginType.NonManipulation))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.IllegalCertificateTypeUpdate, (ECertificateOfOriginType)certificate.TypeId, (ECertificateOfOriginType)agentRequest.CertificateOfOriginTypeCode));
        }

        if (certificate.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.Published or (int)ECertificateOfOriginStatus.Cancelled)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked, (ERequestReason)certificate.RequestReasonCode, (ECertificateOfOriginStatus)certificate.CertificateOfOriginStatusId));
        }

        context.CertificateToUpdateId = certificate.Id;
    }

    // Legacy ValidateCertificateInCertificateReplacement: the cancelled certificate must be Published + a replacement
    // reason must be supplied.
    private static void ValidateCertificateInReplacement(CertificateOfOriginAgentRequestDto agentRequest, CertificateOfOrigin certificate, MessageValidationContext context)
    {
        if (certificate.CertificateOfOriginStatusId != (int)ECertificateOfOriginStatus.Published)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.CertificateToCancelIncorrectStatus, certificate.CertificateNumber));
        }

        if (string.IsNullOrWhiteSpace(agentRequest.ReplacementReason))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ReplacementReasonMissing));
        }
    }

    // Legacy CheckIfCertificatePublishedOrCanceled: a Published/Cancelled certificate cannot be (re)transmitted.
    private async Task CheckIfCertificatePublishedOrCanceled(string certificateNumber, MessageValidationContext context)
    {
        var certificate = await GetExistingCertificate(certificateNumber);
        if (certificate != null
            && certificate.CertificateOfOriginStatusId is (int)ECertificateOfOriginStatus.Published or (int)ECertificateOfOriginStatus.Cancelled)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ItIsNotPossibleToTransmitACertificateThatPublishedOrThatRevoked, (ERequestReason)certificate.RequestReasonCode, (ECertificateOfOriginStatus)certificate.CertificateOfOriginStatusId));
        }
    }

    // Legacy CheckExportDeclarationNumber: the export-declaration number is required, and its lead document must not be
    // in a Draft / CanceledDraft / Canceled state.
    private async Task CheckExportDeclarationNumber(string? declarationNumber, MessageValidationContext context)
    {
        if (string.IsNullOrEmpty(declarationNumber))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ExportDeclarationMissing));
            return;
        }

        var declaration = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(null, declarationNumber);
        if (declaration != null
            && declaration.LeadDocumentStateId is (int)ELeadDocumentState.Canceled or (int)ELeadDocumentState.CanceledDraft or (int)ELeadDocumentState.Draft)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ItIsntPossibleToTransmitDeclarationNumberThatHasntBeenSubmittedOrCanceledDeclaration, declarationNumber));
        }
    }

    // Legacy ChecksIfThereIsDeclarationAssociatedWithTheCertificate: a certificate with an associated (live) declaration
    // cannot be cancelled until the declaration is cancelled first.
    private async Task CheckDeclarationAssociatedWithCertificate(int certificateId, MessageValidationContext context)
    {
        var leadDocument = await exportDealFileProxy.GetLeadDocumentByCertificateOfOriginId(certificateId);
        if (leadDocument != null)
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.TheLinkedDeclarationMustBeCanceledBeforeCancelingTheCertificate, leadDocument.LeadDocumentTitle));
        }
    }

    // Legacy CheckIfCertificateIdToCancelHasValue: certificateIdToCancel is required (Replacement / ImportReplacement).
    private static void CheckCertificateIdToCancelHasValue(string? certificateIdToCancel, MessageValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(certificateIdToCancel))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.ValueNull, "certificateIdToCancel"));
        }
    }

    // Legacy CheckIfCertificateIDHasValue (inverted): for EmptyCertificate the certificateID must be empty.
    private static void CheckCertificateIdIsEmpty(string? certificateId, MessageValidationContext context)
    {
        if (!string.IsNullOrWhiteSpace(certificateId))
        {
            context.Exceptions.Add(BuildMessageException(EMessageCode.MandatoryNullValue, "CertificateID"));
        }
    }

    // Legacy GetCertificateOfOriginByExternalId — the existing certificate by number. No memoization: the BL is a
    // resolved-per-request service, but keeping the fetch stateless (no shared field) is safer; the per-reason checks
    // that repeat a lookup are few, and the legacy itself re-fetched. If profiling shows it matters, memoize on the
    // per-request MessageValidationContext.
    private async Task<CertificateOfOrigin?> GetExistingCertificate(string? certificateNumber)
    {
        if (string.IsNullOrWhiteSpace(certificateNumber))
        {
            return null;
        }

        var certificate = await DataLayer.GetLatestCertificateByNumberForFeedback(certificateNumber);
        return certificate;
    }
}
