using CustomsCloud.InfrastructureCore.Lookup;

namespace CertificateOfOrigins.BL.Lookups;

// TODO(internal): Lookup.PackingType (CargoControl_c_PackingType) is missing from CustomsCloud.InfrastructureCore.Lookup
// 1.10.120. Legacy read it with SystemTablesUtil.GetIdByCode<PackingType>(PropCommonCode, code). Add it to the platform
// package, then delete this record and register services.AddLookup<Lookup.PackingType>().
public record PackingType : ILookup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int State { get; init; }

    public string? Description { get; init; }

    public string? EnglishName { get; init; }

    public string? CommonCode { get; init; }
}
