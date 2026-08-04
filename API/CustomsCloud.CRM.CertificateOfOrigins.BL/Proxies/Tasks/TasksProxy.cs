using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Tasks), ITasksProxy
{
    // Legacy: Container.Resolve<ITasksExternalProxy>().IsTaskExist(IsTaskExistFilter{EntityID, EntityTypeID,
    // TaskTypeIDs}) — the open/handling tasks on an entity live in the Tasks microservice.
    public async Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("Task/IsTaskExist") // TODO(blocking): confirm endpoint name/route with the Tasks microservice
            .AddBody(new { EntityId = entityId, EntityTypeId = entityTypeId, TaskTypeIds = taskTypeIds });
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<TaskExistResultDto>>();
    }
}
