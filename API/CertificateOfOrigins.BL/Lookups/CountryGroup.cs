using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.CountryGroup (General_enum_CountryGroup) is missing from CustomsCloud.InfrastructureCore.Lookup
// 1.10.120. Legacy read it with SystemTablesUtil.GetIdByCode<CountryGroup>(PropID, id) + GetCodeById<CountryGroup>(id). Add it
// to the platform package, then delete this record and register services.AddLookup<Lookup.CountryGroup>().
public record CountryGroup : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public bool IsForTradeAgreement { get; init; }
}
