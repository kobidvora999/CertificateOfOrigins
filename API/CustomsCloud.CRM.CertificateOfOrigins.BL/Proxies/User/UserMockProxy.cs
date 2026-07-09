using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class UserMockProxy : IUserProxy
{
    // Returns hardcoded dummy data — used while the real Users service endpoint is unavailable.
    public Task<List<UserDto>?> GetUsersByIds(List<int> userIds)
    {
        var result = userIds.Select(id => new UserDto
        {
            Id = id,                      // TODO: dummy data
            Title = "Test User " + id,    // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<UserDto>?>(result);
    }
}
