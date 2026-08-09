using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICustomsBookProxy
{
    // Legacy: servicesAdapter.IsTradeAgreementForCountry(certificateTypeId, countryId, isCountryGroup)
    // (SaveCertificateOfOrigin field validation) — whether the country / country-group is party to the trade
    // agreement of the given certificate type.
    Task<bool> IsTradeAgreementForCountry(int certificateTypeId, int countryId, bool isCountryGroup);

    // Legacy: ServicesAdapter.GetCustomsItemsByIdsSync(filters) (UpdateCertificateOfOrigins reconciliation) — resolves
    // the customs items' full tariff classification, used for the 6-digit match between the certificate and the
    // export declaration.
    Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters);
}
