using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class SiteMockProxy(IProxyMockUtil mockUtil) : ISiteProxy, IMockProxy
{
    // Default = a resolved site (with an org-unit id) per requested external number; feature "Site.NotFound" flips to
    // the not-found branch (null → the create branch records "customs house not in table").
    public Task<List<SiteByExternalNumberDto>?> GetSitesByExternalNumbers(List<string> externalSiteNumbers)
    {
        if (mockUtil.HasMockFeature("Site.NotFound"))
        {
            return Task.FromResult<List<SiteByExternalNumberDto>?>(null);
        }

        var result = externalSiteNumbers.Select(number => new SiteByExternalNumberDto
        {
            Id = DummyId(number),                 // TODO: dummy data
            ExternalSiteNumber = number,
            OrganizationUnitId = DummyId(number), // TODO: dummy data
            EnglishName = $"Site {number}",       // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<SiteByExternalNumberDto>?>(result);
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
