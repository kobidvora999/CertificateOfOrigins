namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class ExportDocumentAuthenticationRequestLeadDocumentDto
{
    public int Id { get; set; }
    public int ExportRequestId { get; set; }
    public int? LeadDocumentId { get; set; }
    public string? LeadDocumentTitle { get; set; }
}
