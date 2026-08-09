using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IExportDealFileProxy
{
    Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber);

    // Legacy: the CRP.DealFile_LeadDocumentSubmissionData.SubmitDate JOIN (GetAuthenticationRequestByID) — the lead
    // document's submission date lives in the DealFile service.
    Task<DateTimeOffset?> GetLeadDocumentSubmissionDate(int leadDocumentId);

    // Legacy: IExportDealFileExternalServiceAdapter.GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId
    // (SaveCertificateOfOrigin) — repoints the deal-file lead document from the old certificate to the new one and
    // returns the (now-linked) lead document.
    Task<LeadDocumentByCertificateOfOriginDto?> GetLeadDocumentByOldCertificateOfOriginIdAndUpdateToNewCertificateOfOriginId(int oldCertificateOfOriginId, int newCertificateOfOriginId);

    // Legacy: ChangeCertificateOfOriginIDForLeadDocument — repoints a lead document's certificate id (replacement flow).
    Task ChangeCertificateOfOriginIdForLeadDocument(int leadDocumentId, int oldCertificateOfOriginId, int newCertificateOfOriginId);

    // Legacy: GetExportDeclarationInfoForPC — the export-declaration info used by the declaration-released reconciliation.
    Task<ExportDeclarationInfoDto?> GetExportDeclarationInfoForPc(int leadDocumentId);

    // Legacy: IExportDealFileExternalServiceAdapter.GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId
    // (UpdateCertificateOfOrigins, import-certificate-replacement reconciliation) — the goods items associated with the
    // lead document's import declaration, used to check whether any origin country is in the trade agreement.
    Task<List<ExportAssociatedGoodsItemDto>?> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int leadDocumentId);
}
