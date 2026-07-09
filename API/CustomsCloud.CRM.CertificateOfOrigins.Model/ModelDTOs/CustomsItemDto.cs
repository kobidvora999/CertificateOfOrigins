namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Customs item as returned by the CustomsBook service.
public class CustomsItemDto
{
    public int Id { get; set; }

    public string? FullClassification { get; set; }

    // Computed client-side: first 6 digits of FullClassification (null when shorter than 6).
    public string? FullClassificationBy6Digits { get; set; }
}
