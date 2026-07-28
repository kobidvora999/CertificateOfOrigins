using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CurrencyTypeMockProxy(IProxyMockUtil mockUtil) : ICurrencyTypeProxy, IMockProxy
{
    private static readonly string[] Codes = ["USD", "EUR", "GBP", "ILS"];

    // Default = realistic dummy currency codes; feature "CurrencyType.NotFound" flips to the not-found branch.
    public Task<List<CurrencyTypeDto>?> GetCurrencyTypesByIds(List<int> currencyTypeIds)
    {
        if (mockUtil.HasMockFeature("CurrencyType.NotFound"))
        {
            return Task.FromResult<List<CurrencyTypeDto>?>(null);
        }

        var result = currencyTypeIds.Select(id => new CurrencyTypeDto
        {
            Id = id,                                 // TODO: dummy data
            CurrencyCode = Codes[id % Codes.Length], // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<CurrencyTypeDto>?>(result);
    }
}
