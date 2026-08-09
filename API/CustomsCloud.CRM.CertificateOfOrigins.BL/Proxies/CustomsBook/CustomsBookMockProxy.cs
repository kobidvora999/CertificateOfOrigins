using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomsBookMockProxy(IProxyMockUtil mockUtil) : ICustomsBookProxy, IMockProxy
{
    // Default = the country IS in the trade agreement (validation passes); feature "CustomsBook.NotInAgreement"
    // flips it so the field-validation exception path is exercised.
    public Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isCountryGroup)
    {
        return Task.FromResult(!mockUtil.HasMockFeature("CustomsBook.NotInAgreement"));
    }

    // Default = every requested customs item shares the same 6-digit classification, so the reconciliation 6-digit
    // match passes; feature "CustomsBook.CustomsItemMismatch" gives each item a distinct classification so the
    // customs-item mismatch path is exercised.
    public Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters)
    {
        var mismatch = mockUtil.HasMockFeature("CustomsBook.CustomsItemMismatch");
        var items = filters
            .Where(filter => filter.CustomsItemId.HasValue)
            .Select(filter => filter.CustomsItemId!.Value)
            .Distinct()
            .Select(id => new CustomsItemDto
            {
                Id = id,
                FullClassification = mismatch ? $"{id:D6}0000000" : "1234560000000",
            })
            .ToList();
        return Task.FromResult<List<CustomsItemDto>?>(items);
    }
}
