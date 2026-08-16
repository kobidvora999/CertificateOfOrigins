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
    private async Task<List<CertificateOfOriginExceptionDto>> ReconcileWithSubmittedDeclaration(CertificateOfOriginDto certificate)
    {
        var declarationInfo = await GetDeclarationInfoForReconciliation(certificate);
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

    // Legacy CheckDeclarationStatus (BL.cs:648) with isNeedToReturn:true. Resolve the export declaration for the
    // certificate (by its lead-document id / export-declaration number), then fetch the full declaration info used for
    // the reconciliation. The legacy lead-document repoint (ChangeCertificateOfOriginIDForLeadDocument) is NOT repeated
    // here — SaveCertificateOfOrigin's SupersedePreviousVersion already repointed the lead document during the save.
    private async Task<ExportDeclarationInfoDto?> GetDeclarationInfoForReconciliation(CertificateOfOriginDto certificate)
    {
        if (!certificate.LeadDocumentId.HasValue && string.IsNullOrEmpty(certificate.ExportDeclarationNumber))
        {
            return null;
        }

        var declarationDetails = await exportDealFileProxy.GetExportDeclarationDetailsForCertificateOfOrigion(certificate.LeadDocumentId, certificate.ExportDeclarationNumber);
        if (declarationDetails is null)
        {
            return null;
        }

        var declarationInfo = await exportDealFileProxy.GetExportDeclarationInfoForPc(declarationDetails.LeadDocumentId);
        return declarationInfo;
    }
}
