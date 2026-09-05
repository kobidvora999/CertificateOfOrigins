using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IInternationalSiteProxy
{
    Task<List<InternationalSiteByLocodeDto>?> GetInternationalSitesByLocodes(List<string> locodes);
}
