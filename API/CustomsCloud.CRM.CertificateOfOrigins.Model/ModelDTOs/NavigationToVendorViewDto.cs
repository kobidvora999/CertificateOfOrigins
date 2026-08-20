namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// The "navigate to vendor" view (legacy NavigationToVendorView, Customs.CRM.External). The legacy
// GetPathsForNavigationToVendor populated only PathId + ViewPaths; ViewName / ViewId / IsMandatory stay default
// (preserved bug-for-bug).
public class NavigationToVendorViewDto
{
    public string? ViewName { get; set; }

    public int ViewId { get; set; }

    public int PathId { get; set; }

    public bool IsMandatory { get; set; }

    public List<NavigationToVendorPathDto> ViewPaths { get; set; } = [];
}
