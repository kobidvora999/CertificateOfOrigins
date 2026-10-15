using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CommonServicesMockProxy(IProxyMockUtil mockUtil) : ICommonServicesProxy, IMockProxy
{
    // Default = a minimal dummy JPEG (SOI…EOI markers); feature "Qr.Empty" returns none.
    public Task<byte[]?> CreateQrCode(string url)
    {
        if (mockUtil.HasMockFeature("Qr.Empty"))
        {
            return Task.FromResult<byte[]?>(null);
        }

        return Task.FromResult<byte[]?>([0xFF, 0xD8, 0xFF, 0xD9]); // TODO: dummy JPEG bytes
    }

    // Default = dummy PDF report bytes; feature "Templates.Empty" returns none (no attachments produced). The feature
    // keeps its old name because the Postman mock-features collection sends it.
    public Task<byte[]?> GenerateReportByParams(GenerateReportByParamsRequestDto request)
    {
        if (mockUtil.HasMockFeature("Templates.Empty"))
        {
            return Task.FromResult<byte[]?>(null);
        }

        return Task.FromResult<byte[]?>(Encoding.ASCII.GetBytes("%PDF-1.4\nmock certificate report\n%%EOF")); // TODO: dummy PDF bytes
    }
}
