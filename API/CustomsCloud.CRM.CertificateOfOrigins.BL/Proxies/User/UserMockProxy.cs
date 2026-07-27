using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class UserMockProxy(IProxyMockUtil mockUtil) : IUserProxy, IMockProxy
{
    // Default = realistic dummy users; feature "Users.NotFound" flips to the not-found branch.
    public Task<List<UserDto>?> GetUsersByIds(List<int> userIds)
    {
        if (mockUtil.HasMockFeature("Users.NotFound"))
        {
            return Task.FromResult<List<UserDto>?>(null);
        }

        var result = userIds.Select(id => new UserDto
        {
            Id = id,                      // TODO: dummy data
            Name = "Test User " + id,     // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<UserDto>?>(result);
    }
}
