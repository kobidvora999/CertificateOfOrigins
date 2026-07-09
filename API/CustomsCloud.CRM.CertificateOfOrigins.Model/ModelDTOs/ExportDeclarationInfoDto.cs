namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Returned by ExportDealFile (GetExportDeclarationInfoForPC): the invoices/goods-items graph
// of an export declaration, used to find sibling certificates on the same declaration.
public class ExportDeclarationInfoDto
{
    public int? LeadDocumentId { get; set; }

    public List<ExportInvoiceInfoDto>? ExportInvoiceInfoList { get; set; }
}
