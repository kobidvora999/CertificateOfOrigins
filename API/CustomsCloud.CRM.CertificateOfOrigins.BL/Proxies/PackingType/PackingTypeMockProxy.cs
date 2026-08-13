using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class PackingTypeMockProxy(IProxyMockUtil mockUtil) : IPackingTypeProxy, IMockProxy
{
    // Default = a resolved packing type per requested code; feature "PackingType.NotFound" flips to null.
    public Task<List<PackingTypeByCodeDto>?> GetPackingTypesByCodes(List<string> commonCodes)
    {
        if (mockUtil.HasMockFeature("PackingType.NotFound"))
        {
            return Task.FromResult<List<PackingTypeByCodeDto>?>(null);
        }

        var result = commonCodes.Select(code => new PackingTypeByCodeDto
        {
            Id = DummyId(code),            // TODO: dummy data
            CommonCode = code,
            EnglishName = $"Packing {code}", // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<PackingTypeByCodeDto>?>(result);
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
