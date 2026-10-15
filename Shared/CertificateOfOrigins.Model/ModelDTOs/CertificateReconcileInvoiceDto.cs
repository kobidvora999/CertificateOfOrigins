namespace CertificateOfOrigins.Model.ModelDTOs;

// A certificate invoice + its goods items' customs-item ids, loaded for UpdateCertificateOfOrigins reconciliation
// (the certificate side of the invoice / goods-item matching against the export declaration).
public class CertificateReconcileInvoiceDto
{
    public int CertificateOfOriginId { get; set; }

    public string? InvoiceNumber { get; set; }

    // One entry per goods item, null when the item has no customs item. Legacy ran the per-goods-item checks once for
    // every goods item of the invoice (CertificateOfOriginsBL.cs:809), with or without a customs item, so the items
    // without one are kept here (parity finding BN-01).
    public List<int?> GoodsItemCustomsItemIds { get; set; } = [];
}
