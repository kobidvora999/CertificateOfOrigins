namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginResultDto
{
    public int Id { get; set; }
    public string? CertificateNumber { get; set; }
    public string? Name { get; set; }
    public int CustomesAgentId { get; set; }
    public string? CustomesAgentTitle { get; set; }
    public string? CustomesAgentExternalIdNum { get; set; }
    public int ExporterId { get; set; }
    public string? ExporterTitle { get; set; }
    public string? ExporterExternalIdNum { get; set; }
    public string? ExportDeclarationNumber { get; set; }
    public int VersionNumber { get; set; }
    public int OrganizationUnitId { get; set; }
    public int RequestReasonCode { get; set; }
    public DateTimeOffset? IssuingDate { get; set; }
    public int? LeadDocumentId { get; set; }
}
