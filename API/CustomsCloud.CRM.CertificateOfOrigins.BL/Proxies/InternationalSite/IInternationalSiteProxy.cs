using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IInternationalSiteProxy
{
    Task<List<InternationalSiteByLocodeDto>?> GetInternationalSitesByLocodes(List<string> locodes);
}
