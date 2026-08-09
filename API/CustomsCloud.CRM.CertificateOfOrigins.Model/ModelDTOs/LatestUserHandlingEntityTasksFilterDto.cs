namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Filter for the Tasks service's "latest user handling an entity" lookup (legacy LatestUserHandlingEntityTasksFilter).
// UpdateCertificateOfOrigins uses it to find the export-declaration assessor handling the lead document, to assign the
// declaration-mismatch task to. Only the fields the reconciler sets are carried.
public class LatestUserHandlingEntityTasksFilterDto
{
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }

    public int OrganizationUnitTypeId { get; set; }

    public int OrganizationUnitId { get; set; }
}
