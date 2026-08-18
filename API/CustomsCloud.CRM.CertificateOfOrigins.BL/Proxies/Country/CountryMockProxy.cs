using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CountryMockProxy(IProxyMockUtil mockUtil) : ICountryProxy, IMockProxy
{
    // Default = a resolved country per requested alpha-2 code (deterministic dummy id); feature "Country.NotFound"
    // flips to the not-found branch (null → the create branch records "country not in table").
    public Task<List<CountryByCodeDto>?> GetCountriesByAlphaCodes(List<string> alphaCodes)
    {
        if (mockUtil.HasMockFeature("Country.NotFound"))
        {
            return Task.FromResult<List<CountryByCodeDto>?>(null);
        }

        var result = alphaCodes.Select(code => new CountryByCodeDto
        {
            // "IL" resolves to the seeded Israel id (the CountryIsrael parameter, value 376) so the Israel-country
            // checks pass. Any other code gets a stable non-Israel dummy id.
            Id = string.Equals(code, "IL", StringComparison.OrdinalIgnoreCase) ? 376 : DummyId(code), // TODO: dummy data
            AlphaCode2 = code,
            EnglishName = $"Country {code}", // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<CountryByCodeDto>?>(result);
    }

    // Stable per-code dummy id (avoids GetHashCode's cross-run instability).
    private static int DummyId(string code)
    {
        var sum = 0;
        foreach (var ch in code ?? string.Empty)
        {
            sum += ch;
        }

        return (sum % 1000) + 1;
    }
}
