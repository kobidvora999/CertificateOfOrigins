using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class MessageManagementProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Common), IMessageManagementProxy
{
    // Legacy: Container.Resolve<IMessageManagementExternalProxy>().SendMessage(...) — the Message-Management service
    // lives in the Common microservice.
    public async Task SendMessage(SendMessageDto message)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Message/SendMessage") // TODO(blocking): confirm endpoint name/route with the Message-Management (Common) microservice
            .AddBody(message);
        await ExecuteAsync(req);
    }
}
