using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomsBookProxy
{
    /// <summary>
    /// Checks whether a trade agreement exists for the given certificate type and country.
    /// </summary>
    Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isImport);

    /// <summary>
    /// Returns customs items by ids (with an as-of date per item).
    /// </summary>
    Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters);
}
