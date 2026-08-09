namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A certificate invoice + its goods items' customs-item ids, loaded for UpdateCertificateOfOrigins reconciliation
// (the certificate side of the invoice / goods-item matching against the export declaration).
public class CertificateReconcileInvoiceDto
{
    public int CertificateOfOriginId { get; set; }

    public string? InvoiceNumber { get; set; }

    public List<int> CustomsItemIds { get; set; } = [];
}
