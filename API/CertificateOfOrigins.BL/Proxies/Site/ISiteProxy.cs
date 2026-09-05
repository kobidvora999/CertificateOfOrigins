using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ISiteProxy
{
    Task<List<SiteByExternalNumberDto>?> GetSitesByExternalNumbers(List<string> externalSiteNumbers);
}
