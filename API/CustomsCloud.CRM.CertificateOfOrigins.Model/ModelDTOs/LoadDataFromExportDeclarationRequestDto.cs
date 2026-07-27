namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Request for LoadDataFromExportDeclaration — the certificate-of-origin fields needed to look up its export
// declaration in the ExportDealFile service. The legacy passed the whole CertificateOfOrigin entity by
// reference; only these three fields were actually read.
public class LoadDataFromExportDeclarationRequestDto
{
    public int? LeadDocumentId { get; set; }
    public string? ExportDeclarationNumber { get; set; }
    public int RequestReasonCode { get; set; }
}
