namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string? Title { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public string? ExternalIdNum { get; set; }
    public string? Notes { get; set; }

    // Not returned by the SP — set in BL from DocumentType.Name (lookup by TypeId)
    public string? FileUrl { get; set; }
}
