using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ICountryProxy
{
    Task<List<CountryByCodeDto>?> GetCountriesByAlphaCodes(List<string> alphaCodes);
}
