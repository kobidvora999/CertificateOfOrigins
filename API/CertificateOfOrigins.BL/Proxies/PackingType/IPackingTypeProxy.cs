using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IPackingTypeProxy
{
    Task<List<PackingTypeByCodeDto>?> GetPackingTypesByCodes(List<string> commonCodes);
}
