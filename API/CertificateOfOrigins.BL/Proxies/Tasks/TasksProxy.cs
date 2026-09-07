using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksProxy(IHttpProxy httpProxy)
    : BaseCustomsProxy(httpProxy, CustomsMicroServices.Tasks), ITasksProxy
{
    // Legacy: Container.Resolve<ITasksExternalProxy>().IsTaskExist(IsTaskExistFilter{EntityID, EntityTypeID,
    // TaskTypeIDs, IsTaskInProgress}) — the tasks on an entity live in the Tasks microservice.
    public async Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds, bool isTaskInProgress = false)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Task/IsTaskExist") // TODO(blocking): confirm endpoint name/route with the Tasks microservice
            .AddBody(new { EntityId = entityId, EntityTypeId = entityTypeId, TaskTypeIds = taskTypeIds, IsTaskInProgress = isTaskInProgress });
        var response = await ExecuteAsync(req);
        return await response.GetResult<List<TaskExistResultDto>>();
    }

    // Legacy: ITasksExternalProxy.IsTaskExistsOnEntity(VirtualEntity, taskTypeID) → bool, with TasksBL pinning
    // IsTaskInProgress = true. Restored as its own operation rather than folded into IsTaskExist, because that
    // fold is what lost the open-only semantics in the first place.
    public async Task<bool> IsTaskExistsOnEntity(int entityId, int entityTypeId, int taskTypeId)
    {
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Task/IsTaskExistsOnEntity") // TODO(blocking): confirm endpoint name/route with the Tasks microservice
            .AddBody(new { EntityId = entityId, EntityTypeId = entityTypeId, TaskTypeId = taskTypeId });
        var response = await ExecuteAsync(req);
        return await response.GetResult<bool>();
    }

    public async Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter)
    {
        // TODO(blocking): confirm endpoint name/route with the Tasks microservice. The real endpoint returns a
        // LatestUserHandlingEntityTasksResult whose UserID is the assessor (legacy adapter returned result.UserID).
        var req = CreateRequestBuilder()
            .UsePostMethod()
            .WithResource("api/Task/LatestUserHandlingEntityTasksWithTaskUnification")
            .AddBody(filter);
        var response = await ExecuteAsync(req);
        return await response.GetResult<int?>();
    }
}
