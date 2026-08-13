using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ISiteProxy
{
    Task<List<SiteByExternalNumberDto>?> GetSitesByExternalNumbers(List<string> externalSiteNumbers);
}
