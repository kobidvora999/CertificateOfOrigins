namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportInvoiceInfoDto
{
    public int SequenceNumber { get; set; }

    public int InvoiceId { get; set; }

    public string? ExternalIdNum { get; set; }

    public DateTime? IssueDate { get; set; }

    public List<ExportGoodsItemInfoDto>? ExportGoodsItemInfoList { get; set; }
}
