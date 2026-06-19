namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

public class CustomerActivityDto
{
    public int ActivityId { get; set; }
    public int ActiveActivityTypeId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public string? LogisticIdentification { get; set; }
    public List<int> ActivityIndicationTypeIds { get; set; } = new();
}
