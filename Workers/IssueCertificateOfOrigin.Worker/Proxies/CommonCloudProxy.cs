using IssueCertificateOfOrigin.Worker.Dto;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    // On IHttpProxy / BaseCustomsProxy: BaseMicroServiceProxyAdapter was removed from InfrastructureCore.Proxy after 1.10.86.
    public class CommonCloudProxy(IHttpProxy proxy) : BaseCustomsProxy(proxy, CustomsMicroServices.Common), ICommonCloudProxy
    {
        public async Task SendOutgoingMessage(OutgoingMessageRequest request)
        {
            var builder = CreateRequestBuilder()
                .WithResource("OutgoingMessage")
                .UsePostMethod()
                .AddBody(request);

            await ExecuteAsync(builder);
        }
    }
}
