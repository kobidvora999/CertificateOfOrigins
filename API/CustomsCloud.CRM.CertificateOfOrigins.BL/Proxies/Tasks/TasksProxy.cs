using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksProxy(IRestProxy restProxy)
    : BaseMicroServiceProxyAdapter(restProxy, CustomsMicroServices.Tasks), ITasksProxy // TODO: confirm CustomsMicroServices.Tasks exists in the enum
{
    public async Task<List<IsTaskExistResultDto>?> IsTaskExist(IsTaskExistFilterDto filter)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Tasks/TaskExist") // TODO: confirm endpoint name + verb (route = target BL name) with the Tasks microservice
            .AddBody(filter);
        return (await ExecuteAsync<List<IsTaskExistResultDto>>(req)).Data;
    }

    public async Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Tasks/LatestUserHandlingEntityTasksWithTaskUnification") // TODO(blocking): confirm endpoint name with the Tasks microservice
            .AddBody(filter);
        return (await ExecuteAsync<int?>(req)).Data;
    }
}
