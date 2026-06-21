namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors Customs.Infrastructure.Tasks IsTaskExistFilter.
public class IsTaskExistFilterDto
{
    public List<int> TaskTypeIds { get; set; } = new();
    public bool IsTaskInProgress { get; set; }
    public int EntityTypeId { get; set; }
    public int EntityId { get; set; }
}
