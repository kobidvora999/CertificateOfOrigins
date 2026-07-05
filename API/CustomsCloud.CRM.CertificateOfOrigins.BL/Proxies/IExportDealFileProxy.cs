using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IExportDealFileProxy
{
    /// <summary>
    /// Returns export-declaration details (release / cargo-exit indications) for a certificate of origin,
    /// located by lead document id or by export declaration number.
    /// </summary>
    Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigin(int? leadDocumentId, string? exportDeclarationNumber);
}
