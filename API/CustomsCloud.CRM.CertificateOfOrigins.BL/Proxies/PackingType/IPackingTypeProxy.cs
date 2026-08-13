using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IPackingTypeProxy
{
    Task<List<PackingTypeByCodeDto>?> GetPackingTypesByCodes(List<string> commonCodes);
}
