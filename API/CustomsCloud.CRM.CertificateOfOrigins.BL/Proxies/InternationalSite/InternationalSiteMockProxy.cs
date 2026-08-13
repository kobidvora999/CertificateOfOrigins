using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class InternationalSiteMockProxy(IProxyMockUtil mockUtil) : IInternationalSiteProxy, IMockProxy
{
    // Default = a resolved international site per requested locode; feature "InternationalSite.NotFound" flips to null.
    public Task<List<InternationalSiteByLocodeDto>?> GetInternationalSitesByLocodes(List<string> locodes)
    {
        if (mockUtil.HasMockFeature("InternationalSite.NotFound"))
        {
            return Task.FromResult<List<InternationalSiteByLocodeDto>?>(null);
        }

        var result = locodes.Select(locode => new InternationalSiteByLocodeDto
        {
            Id = DummyId(locode),           // TODO: dummy data
            Locode = locode,
            EnglishName = $"Site {locode}", // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<InternationalSiteByLocodeDto>?>(result);
    }

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
