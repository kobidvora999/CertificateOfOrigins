namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CertificateOfOriginFilterDto
{
    public string? CertificateNumber { get; set; }
    public int? CertificateOfOriginStatusId { get; set; }
    public int? CertificateOfOriginTypeId { get; set; }
    public int? CustomsAgentId { get; set; }
    public int? CustomsHouseId { get; set; }
    public int? DestinationCountry { get; set; }
    public int? ExportDeclarationId { get; set; }
    public string? ExportDeclarationNum { get; set; }
    public int? ExporterCustomerId { get; set; }
    public DateTime? FromIssuingDate { get; set; }
    public DateTime? ToIssuingDate { get; set; }
    public DateTime? FromRequestDate { get; set; }
    public DateTime? ToRequestDate { get; set; }
    public int? RequestReasonId { get; set; }
    public int? VersionNumber { get; set; }
    public bool? IsLastVersion { get; set; }
}
