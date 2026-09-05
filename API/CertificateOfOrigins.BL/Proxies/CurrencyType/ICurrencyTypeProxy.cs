using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ICurrencyTypeProxy
{
    Task<List<CurrencyTypeDto>?> GetCurrencyTypesByIds(List<int> currencyTypeIds);

    Task<List<CurrencyTypeDto>?> GetCurrencyTypesByCodes(List<string> currencyCodes);
}
