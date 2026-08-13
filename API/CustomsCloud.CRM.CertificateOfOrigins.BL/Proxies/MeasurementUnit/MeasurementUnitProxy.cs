using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class MeasurementUnitProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), IMeasurementUnitProxy
{
    // Legacy: SystemTablesUtil.GetIdByCode<MeasurementUnit>(PropExternalIDNum, code) — the invoice item MeasureType
    // codes are resolved to measurement-unit ids by the SystemTables service. Batched by the message codes.
    public async Task<List<MeasurementUnitByCodeDto>?> GetMeasurementUnitsByCodes(List<string> externalIdNumbers)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("MeasurementUnit/MeasurementUnitsByCodes") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(externalIdNumbers);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<MeasurementUnitByCodeDto>>();
    }
}
