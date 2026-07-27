using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface IUserProxy
{
    Task<List<UserDto>?> GetUsersByIds(List<int> userIds);
}
