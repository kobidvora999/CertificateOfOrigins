using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IExportDealFileProxy
{
    Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigion(int? leadDocumentId, string? exportDeclarationNumber);
}
