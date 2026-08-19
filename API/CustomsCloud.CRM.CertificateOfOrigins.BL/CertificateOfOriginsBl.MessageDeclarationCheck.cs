using CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL;

// GetPC_MSG2280_2281 create branch — the post-save declaration-submitted reconciliation (legacy
// CheckCertificateOfOriginOnDeclarationSubmited → CheckDeclarationStatus → UpdateCertrificateOfOrigins). After the
// certificate is saved (reason != EmptyCertificate, type != NonManipulation), if the linked export declaration is
// Submited/Released/Paroled/Closed, reconcile the certificate against the declaration's invoices/goods-items via the
// migrated UpdateCertificateOfOrigins (#34), and return any mismatch exceptions for the feedback response.
public partial class CertificateOfOriginsBl
{
    // Legacy CheckCertificateOfOriginOnDeclarationSubmited (BL.cs:695): gate on the declaration state, then delegate to
    // the reconciliation. Returns the reconciliation exceptions (empty when the gate did not fire).
    private async Task<List<CertificateOfOriginExceptionDto>> ReconcileWithSubmittedDeclaration(CertificateOfOriginDto certificate, MessageValidationContext context)
    {
        var declarationInfo = await GetDeclarationInfoForReconciliation(certificate, context);
        if (declarationInfo is null
            || declarationInfo.LeadDocumentState is not ((int)ELeadDocumentState.Submited
                or (int)ELeadDocumentState.Released
                or (int)ELeadDocumentState.Paroled
                or (int)ELeadDocumentState.Closed))
        {
            return [];
        }

        // Legacy: build the UpdateCetificateOfOriginsDTO from the declaration info + the just-saved certificate, then
        // run the reconciliation (isFromDealFile:false → single certificate). EventType is left unset (legacy default).
        var updateRequest = new UpdateCertificateOfOriginsRequestDto
        {
            CertificateOfOriginsIds = [certificate.Id],
            DestinationCountryId = declarationInfo.DestinationCountryId,
            ExporterCustomerId = declarationInfo.ExporterCustomerId,
            ExportDeclarationNum = certificate.ExportDeclarationNumber,
            LeadDocumentId = declarationInfo.LeadDocumentId,
            OrganizationUnitId = declarationInfo.OrganizationUnitId ?? 0,
            ExportInvoiceInfoList = declarationInfo.ExportInvoiceInfoList,
        };

        var exceptions = await UpdateCertificateOfOrigins(updateRequest);
        return exceptions;
    }

    // Legacy CheckIfCertificateIsLinkedToDeclarationInAmendment (Inner method, before the branch): block the request
    // when the linked export declaration is in an amendment process. Accumulated in-band (not thrown).
    private async Task CheckDeclarationNotInAmendment(string? exportDeclarationNumber, MessageValidationContext context, List<CertificateOfOriginExceptionDto> requestExceptions)
    {
        if (string.IsNullOrEmpty(exportDeclarationNumber))
        {
            return;
        }

        var declarationDetails = await GetExportDeclarationDetailsCached(null, exportDeclarationNumber, context);
        if (declarationDetails is { IsDeclarationInAmendmentProcess: true })
        {
            requestExceptions.Add(BuildMessageException(EMessageCode.CertificateCannotBeTransmittedWhenThereIsAmendmentProcessOnTheDeclaration, exportDeclarationNumber));
        }
    }

    // Legacy CheckDeclarationStatus (BL.cs:648) with isNeedToReturn:true. Resolve the export declaration for the
    // certificate (by its lead-document id / export-declaration number), then fetch the full declaration info used for
    // the reconciliation. The legacy lead-document repoint (ChangeCertificateOfOriginIDForLeadDocument) is NOT repeated
    // here — SaveCertificateOfOrigin's SupersedePreviousVersion already repointed the lead document during the save.
    private async Task<ExportDeclarationInfoDto?> GetDeclarationInfoForReconciliation(CertificateOfOriginDto certificate, MessageValidationContext context)
    {
        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        if (!certificate.LeadDocumentId.HasValue && string.IsNullOrEmpty(certificate.ExportDeclarationNumber))
        {
            return null;
        }

        var declarationDetails = await GetExportDeclarationDetailsCached(certificate.LeadDocumentId, certificate.ExportDeclarationNumber, context);
        if (declarationDetails is null)
        {
            return null;
        }

        var declarationInfo = await exportDealFileProxy.GetExportDeclarationInfoForPc(declarationDetails.LeadDocumentId);
        return declarationInfo;
    }

    // Per-request memoized fetch of GetExportDeclarationDetailsForCertificateOfOrigion — a single message hits this
    // service up to 3× on the same declaration (amendment guard + per-reason check share the (null, number) key). The
    // declaration does not change within a request, so the cached result is returned on repeat keys.
    private async Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsCached(int? leadDocumentId, string? exportDeclarationNumber, MessageValidationContext context)
    {
        var key = $"{leadDocumentId}|{exportDeclarationNumber}";
        if (context.DeclarationDetailsCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var exportDealFileProxy = Resolve<IExportDealFileProxy>();
        var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(leadDocumentId, exportDeclarationNumber);
        context.DeclarationDetailsCache[key] = declarationDetails;
        return declarationDetails;
    }
}
