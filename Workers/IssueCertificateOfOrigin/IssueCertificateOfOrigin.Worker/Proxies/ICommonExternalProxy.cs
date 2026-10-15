using CommonServicesExternal;
using CustomsCloud.InfrastructureCore.Proxy;

namespace IssueCertificateOfOrigin.Worker.Proxies
{
    public interface ICommonExternalProxy
    {
        Task<GenerateReportByParamsResponse?> GenerateReportAsync(GenerateReportByParamsRequest generateReportByParamsRequest);
    }
}
