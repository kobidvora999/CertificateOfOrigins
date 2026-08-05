using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class MessageManagementMockProxy(IProxyMockUtil mockUtil) : IMessageManagementProxy, IMockProxy
{
    // No-op send for local/testing (the real send hits the Message-Management microservice); the "MessageManagement.Fail"
    // feature simulates a transport failure.
    public Task SendMessage(SendMessageDto message)
    {
        if (mockUtil.HasMockFeature("MessageManagement.Fail"))
        {
            throw new InvalidOperationException("Mock: Message-Management send failed.");
        }

        return Task.CompletedTask;
    }
}
