using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class UserProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Users), IUserProxy
{
    public async Task<List<UserDto>?> GetUsersByIds(List<int> userIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("User/UsersByIds") // TODO(blocking): confirm endpoint name/route with the Users microservice
            .AddBody(userIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<UserDto>>();
    }
}
