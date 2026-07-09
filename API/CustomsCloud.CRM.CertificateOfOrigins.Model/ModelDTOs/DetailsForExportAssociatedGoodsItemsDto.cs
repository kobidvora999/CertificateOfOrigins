namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Returned by ExportDealFile: details of goods items associated to an import declaration,
// used to check trade-agreement linkage on ImportCertificateReplacement requests.
public class DetailsForExportAssociatedGoodsItemsDto
{
    public int AssociatedOriginCountryId { get; set; }
}
