using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

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

    // Default = a dummy PDF template result; feature "Templates.Empty" returns none (no attachments produced).
    public Task<TemplateResultDto?> GenerateTemplate(int templateId, int certificateOfOriginId, string additionalInfo)
    {
        if (mockUtil.HasMockFeature("Templates.Empty"))
        {
            return Task.FromResult<TemplateResultDto?>(null);
        }

        var result = new TemplateResultDto
        {
            Id = templateId,
            Name = $"Mock certificate template {templateId}",           // TODO: dummy data
            Content = Encoding.ASCII.GetBytes("%PDF-1.4\nmock certificate\n%%EOF"), // TODO: dummy PDF bytes
            DocumentTypeId = 0,                                          // TODO: dummy data
            IsPdfFormat = true,
        };
        return Task.FromResult<TemplateResultDto?>(result);
    }
}
