using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CommonServicesProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Common), ICommonServicesProxy
{
    public async Task<byte[]?> CreateQrCode(string url)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("CommonServices/CreateQrCode") // TODO(blocking): confirm endpoint name/route with the Common (QR) microservice
            .AddBody(url);
        var response = await ExecuteAsync(req);
        return await response.GetResult<byte[]>();
    }

    public async Task<TemplateResultDto?> GenerateTemplate(int templateId, int certificateOfOriginId, string additionalInfo)
    {
        // TODO(blocking): the Templates generation service + the certificate-of-origin templates are not yet migrated
        // — confirm the route and run template-migrate for the real templates (until then the mock is used).
        var req = CreateRequestBuilder()
            .UseGetMethod()
            .WithResource($"CommonServices/GenerateTemplate/{templateId}/{certificateOfOriginId}")
            .AddQueryStringParameter("additionalInfo", additionalInfo ?? string.Empty);
        var response = await ExecuteAsync(req);
        return await response.GetResult<TemplateResultDto>();
    }
}
