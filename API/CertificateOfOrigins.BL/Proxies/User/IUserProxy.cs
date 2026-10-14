using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface IUserProxy
{
    Task<List<UserDto>?> GetUsersByIds(List<int> userIds);

    // The letter signature block. Null when the user has no template details.
    Task<UserTemplateDetailsDto?> GetUserTemplateDetails(int userId);
}
