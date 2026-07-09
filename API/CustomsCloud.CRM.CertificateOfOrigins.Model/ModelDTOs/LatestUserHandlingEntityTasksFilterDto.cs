namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Filter for Tasks service: latest user handling the entity's tasks (with task unification).
public class LatestUserHandlingEntityTasksFilterDto
{
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }

    public int OrganizationUnitTypeId { get; set; }

    public int OrganizationUnitId { get; set; }
}
