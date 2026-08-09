namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A goods item associated with an import declaration, returned by the ExportDealFile service for an
// import-certificate replacement (legacy GetDetailsForExportAssociatedGoodsItemsByLeadDocumentId). Reconciliation
// checks whether any associated item's origin country is party to the certificate type's trade agreement.
public class ExportAssociatedGoodsItemDto
{
    public int AssociatedOriginCountryId { get; set; }
}
