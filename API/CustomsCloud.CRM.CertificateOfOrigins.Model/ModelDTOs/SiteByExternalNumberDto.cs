namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// A site resolved by its external site number (legacy SystemTablesUtil.GetIdByCode<SiteLookup>(
// PropExternalSiteNumberForMessages, code) → GetCodeById<SiteLookup>(id).OrganizationUnitID). The create branch resolves
// the CustomsHouse field's external site number to the site's org-unit id (then verifies it's a customs house).
// Resolved via the SystemTables microservice (no ILookupUtil type exposes the external site number → org-unit mapping).
public class SiteByExternalNumberDto
{
    public int Id { get; set; }

    public string? ExternalSiteNumber { get; set; }

    public int? OrganizationUnitId { get; set; }

    public string? EnglishName { get; set; }
}
