using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ICustomsBookProxy
{
    // Legacy: CustomsBookServicesAdapter.IsTradeAgreementForCountry(countryId, tradeAgreementId, isCountryGroup) — whether
    // the country / country-group is party to ONE trade agreement. The certificate-type logic around it (Israel, the type's
    // agreements) is the BL's IsTradeAgreementForCountry, as legacy ServicesAdapter held it.
    Task<bool> IsTradeAgreementForCountry(int countryOrGroupId, int tradeAgreementId, bool isGroup);

    // Legacy: ServicesAdapter.GetCustomsItemsByIdsSync(filters) (UpdateCertificateOfOrigins reconciliation) — resolves
    // the customs items' full tariff classification, used for the 6-digit match between the certificate and the
    // export declaration.
    Task<List<CustomsItemDto>?> GetCustomsItemsByIds(List<CustomsItemsIdsCacheFilterDto> filters);

    // Legacy: servicesAdapter.GetCustomsItemIdByFullClassification(fullClassification, ECustomsBookType.Export)
    // (GetPC_MSG2280_2281 create-branch item validation) — resolve a full tariff classification to its customs-item id
    // in the export customs book (null when it does not exist).
    Task<int?> GetCustomsItemIdByFullClassification(string fullClassification);
}
