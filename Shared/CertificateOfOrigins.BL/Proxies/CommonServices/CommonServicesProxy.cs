using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class CommonServicesProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Common), ICommonServicesProxy
{
    public async Task<byte[]?> CreateQrCode(string url)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CommonServices/CreateQrCode") // TODO(blocking): confirm endpoint name/route with the Common (QR) microservice
            .AddBody(url);
        var response = await ExecuteAsync(req);
        return await response.GetResult<byte[]>();
    }

    public async Task<byte[]?> GenerateReportByParams(GenerateReportByParamsRequestDto request)
    {
        // TODO(blocking): confirm the route of GenerateReportByParams on the Common (SSRS reports) microservice.
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/CommonServices/GenerateReportByParams")
            .AddBody(request);
        var response = await ExecuteAsync(req);
        return await response.GetResult<byte[]>();
    }
}
