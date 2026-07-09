using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CommonProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Common), ICommonProxy
{
    public async Task<byte[]?> CreateQRCode(string url)
    {
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource("Common/CreateQRCode") // TODO(blocking): confirm endpoint name against the Common service
            .AddQueryStringParameter("url", url);
        return (await ExecuteAsync<byte[]>(req)).Data;
    }

    public async Task<string?> SendMessageToAgent(MessageToAgentDto message)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Common/MessageToAgent") // TODO(blocking): confirm endpoint name against the Common service
            .AddBody(message);
        return (await ExecuteAsync<string>(req)).Data;
    }
}
