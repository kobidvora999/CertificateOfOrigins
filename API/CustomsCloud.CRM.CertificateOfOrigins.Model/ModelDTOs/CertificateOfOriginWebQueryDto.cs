namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Materialized graph of dbo.GetCertificateOfOriginDataForWebQuery (5 result sets: 1 header · 2 invoices ·
// 3 details · 4 detail-type-code lookup · 5 web print-out). Composed in the DbContext extension; the BL
// (GetCertificateRequestByGuid) transforms it into CertificateOfOriginsResponseDto. Only the header columns the
// BL consumes are declared — Dapper ignores the rest. DocumentId is NULL from the SP (the cross-service
// Infrastructure.Docs_* JOIN was removed); see the BL TODO(blocking).
public class CertificateOfOriginWebQueryDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public int? CertificateIdToCancel { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? IssuingDate { get; set; }
    public int RequestReasonCode { get; set; }
    public string? ExportDeclarationNumber { get; set; }
    public Guid? Guid { get; set; }
    public int? DocumentId { get; set; }

    public List<CertificateOfOriginWebDetailDto> CertificateOfOriginDetails { get; set; } = [];
    public List<CertificateOfOriginInvoiceDetailDto> CertificateOfOriginInvoiceDetail { get; set; } = [];
}
