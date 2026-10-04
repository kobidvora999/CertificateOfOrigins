using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.Site exists in CustomsCloud.InfrastructureCore.Lookup 1.10.120 but lacks the two fields legacy read
// with SystemTablesUtil.GetIdByCode<SiteLookup>(PropExternalSiteNumberForMessages, code) + GetCodeById<SiteLookup>(id)
// .OrganizationUnitID. Extend the platform Lookup.Site with ExternalSiteNumberForMessages and OrganizationUnitId, then delete
// this record and register services.AddLookup<Lookup.Site>().
public record Site : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public int TypeId { get; init; }

    public string? ExternalSiteNumberForMessages { get; init; }

    public int? OrganizationUnitId { get; init; }
}
