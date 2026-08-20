namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// One navigation-path entry in the "navigate to vendor" tree (legacy NavigationToVendorPath, Customs.CRM.External).
// Name resolves to the page name when PageNameId is set, otherwise the view name (legacy mapping).
public class NavigationToVendorPathDto
{
    public int Id { get; set; }

    public int PathId { get; set; }

    public int? PageNameId { get; set; }

    public string? Name { get; set; }

    public int? ViewId { get; set; }

    public int? ParentPathRouteId { get; set; }
}
