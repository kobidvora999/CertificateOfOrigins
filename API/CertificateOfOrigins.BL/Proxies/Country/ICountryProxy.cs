using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ICountryProxy
{
    Task<List<CountryByCodeDto>?> GetCountriesByAlphaCodes(List<string> alphaCodes);
}
