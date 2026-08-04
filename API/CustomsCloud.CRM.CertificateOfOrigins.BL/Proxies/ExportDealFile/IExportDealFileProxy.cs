using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IExportDealFileProxy
{
    Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber);

    // Legacy: the CRP.DealFile_LeadDocumentSubmissionData.SubmitDate JOIN (GetAuthenticationRequestByID) — the lead
    // document's submission date lives in the DealFile service.
    Task<DateTimeOffset?> GetLeadDocumentSubmissionDate(int leadDocumentId);
}
