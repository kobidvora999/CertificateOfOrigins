using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.InternationalSite is missing from CustomsCloud.InfrastructureCore.Lookup 1.10.120. Legacy read it
// with SystemTablesUtil.GetIdByCode<InternationalSite>(PropLocode, code) + GetCodeById<InternationalSite>(id). Add it to the
// platform package (with Locode), then delete this record and register services.AddLookup<Lookup.InternationalSite>().
public record InternationalSite : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public string? Locode { get; init; }
}
