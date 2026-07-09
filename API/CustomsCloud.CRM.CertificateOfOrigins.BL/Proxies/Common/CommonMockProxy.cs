using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CommonMockProxy : ICommonProxy
{
    // Returns hardcoded dummy data — used while the Common service is unavailable.
    public Task<byte[]?> CreateQRCode(string url)
    {
        return Task.FromResult<byte[]?>(Encoding.ASCII.GetBytes($"MOCK-QR:{url}"));
    }

    public Task<string?> SendMessageToAgent(MessageToAgentDto message)
    {
        return Task.FromResult<string?>("MOCK-OK");
    }
}
