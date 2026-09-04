using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class DataDictionaryFieldProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.SystemTables), IDataDictionaryFieldProxy
{
    // Legacy: SystemTablesUtil.GetCodeById<DataDictionaryField>(fieldId).EnglishName — the field labels live in the
    // SystemTables service. Batched by ids (only FieldIDs 20306/20310/20661 are used by the web query).
    public async Task<List<DataDictionaryFieldDto>?> GetDataDictionaryFieldsByIds(List<int> fieldIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/DataDictionaryField/DataDictionaryFieldsByIds") // TODO(blocking): confirm endpoint name/route with the SystemTables microservice
            .AddBody(fieldIds);
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<DataDictionaryFieldDto>>();
    }
}
