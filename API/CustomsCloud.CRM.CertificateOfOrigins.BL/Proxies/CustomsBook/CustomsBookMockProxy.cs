using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CustomsBookMockProxy : ICustomsBookProxy
{
    // Returns hardcoded dummy data — used while the CustomsBook service is unavailable.
    public Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isImport)
    {
        return Task.FromResult(true);
    }

    public Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters)
    {
        var result = filters.Where(f => f.CustomsItemId.HasValue).Select(f => new CustomsItemDto
        {
            Id = f.CustomsItemId!.Value,
            FullClassification = "12345678"
        }).ToList();
        return Task.FromResult<List<CustomsItemDto>?>(result);
    }
}
