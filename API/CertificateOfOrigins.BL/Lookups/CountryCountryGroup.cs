using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.CountryCountryGroup (the link table General_cl_CountryCountryGroup: which country belongs to which
// country group) is missing from CustomsCloud.InfrastructureCore.Lookup 1.10.120. Legacy read it with
// SystemTablesUtil.GetTablesSync<CountryCountryGroup>(IgnoreState = true, CountryID + CountryGroupID). A link table has no
// name or state of its own; the ILookup members are carried empty. Add it to the platform package, then delete this record
// and register services.AddLookup<Lookup.CountryCountryGroup>().
public record CountryCountryGroup : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public int CountryId { get; init; }

    public int CountryGroupId { get; init; }
}
