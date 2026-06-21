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
            .WithResource("External/IsTaskExist") // TODO: confirm endpoint name + verb with the Tasks microservice
            .AddBody(filter);
        return (await ExecuteAsync<List<IsTaskExistResultDto>>(req)).Data;
    }
}
