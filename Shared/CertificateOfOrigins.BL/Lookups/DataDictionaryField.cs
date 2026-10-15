using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.DataDictionaryField (the DataDictionary module's field catalogue, Customs.DataDictionary.Entities)
// is missing from CustomsCloud.InfrastructureCore.Lookup 1.10.120. Legacy read it with
// SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldId).EnglishName (the web-query field labels). Add it to the
// platform package, then delete this record and register services.AddLookup<Lookup.DataDictionaryField>().
public record DataDictionaryField : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }
}
