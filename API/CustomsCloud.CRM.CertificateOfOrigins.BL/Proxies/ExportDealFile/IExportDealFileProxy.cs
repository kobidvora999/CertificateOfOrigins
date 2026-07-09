using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IExportDealFileProxy
{
    /// <summary>
    /// Returns export-declaration details (release / cargo-exit indications) for a certificate of origin,
    /// located by lead document id or by export declaration number.
    /// </summary>
    Task<ExportDeclarationDetailsDto?> GetExportDeclarationDetailsForCertificateOfOrigin(int? leadDocumentId, string? exportDeclarationNumber);

    /// <summary>
    /// Details of goods items associated to an import declaration by lead document id —
    /// used to check trade-agreement linkage on ImportCertificateReplacement requests.
    /// </summary>
    Task<List<DetailsForExportAssociatedGoodsItemsDto>?> GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId(int? leadDocumentId);

    /// <summary>
    /// The invoices/goods-items graph of an export declaration — used to find sibling
    /// certificates on the same declaration.
    /// </summary>
    Task<ExportDeclarationInfoDto?> GetExportDeclarationInfoForPC(int? leadDocumentId);
}
