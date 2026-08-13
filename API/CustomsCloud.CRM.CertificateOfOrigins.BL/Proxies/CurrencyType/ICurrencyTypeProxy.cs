using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICurrencyTypeProxy
{
    Task<List<CurrencyTypeDto>?> GetCurrencyTypesByIds(List<int> currencyTypeIds);

    Task<List<CurrencyTypeDto>?> GetCurrencyTypesByCodes(List<string> currencyCodes);
}
