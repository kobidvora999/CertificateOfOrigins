using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IUserProxy
{
    Task<List<UserDto>?> GetUsersByIds(List<int> userIds);
}
