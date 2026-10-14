using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

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
            Title = "Test User " + id,     // TODO: dummy data
            OrganizationUnit = 6,         // TODO: dummy data (e.g. חיפה)
        }).ToList();
        return Task.FromResult<List<UserDto>?>(result);
    }

    // Default = a full signature block; feature "Users.NoTemplateDetails" flips to a user with no template details.
    public Task<UserTemplateDetailsDto?> GetUserTemplateDetails(int userId)
    {
        if (mockUtil.HasMockFeature("Users.NoTemplateDetails"))
        {
            return Task.FromResult<UserTemplateDetailsDto?>(null);
        }

        var result = new UserTemplateDetailsDto
        {
            UserId = userId,
            EnglishFirstName = "Test",                                          // TODO: dummy data
            EnglishLastName = "User " + userId,                                 // TODO: dummy data
            SignatureImageUrl = "signature-" + userId + ".png",                 // TODO: dummy data
            Email = "test.user" + userId + "@example.invalid",                  // TODO: dummy data
        };
        return Task.FromResult<UserTemplateDetailsDto?>(result);
    }
}
