using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.CurrencyType (General_enum_CurrencyType) is missing from CustomsCloud.InfrastructureCore.Lookup
// 1.10.120. Legacy read it with SystemTablesUtil.GetCodeById<CurrencyType>(id).CurrencyCode and
// GetIdByCode<CurrencyType>(PropCurrencyCode, code). Add it to the platform package, then delete this record and register
// services.AddLookup<Lookup.CurrencyType>().
public record CurrencyType : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public string? CurrencyCode { get; init; }
}
