using CommonServicesExternal;
using CustomsCloud.InfrastructureCore.Proxy;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    public sealed class CommonExternalProxy(ICustomsWcfProxy<ICommonServicesExternalContract> proxy) : ICommonExternalProxy
    {
        public async Task<GenerateReportByParamsResponse?> GenerateReportAsync(GenerateReportByParamsRequest generateReportByParamsRequest)
        {
            var response = await proxy.InvokeAsync(c => c.GenerateReportByParamsAsync(generateReportByParamsRequest));
            return response;
        }
    }
}
