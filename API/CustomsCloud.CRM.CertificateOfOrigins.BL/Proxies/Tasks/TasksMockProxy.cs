using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksMockProxy(IProxyMockUtil mockUtil) : ITasksProxy, IMockProxy
{
    // Default = one in-progress task per requested type, owned by user 5 (matches the CC-USER-ID used in local
    // testing, so IsCurrentUserHandleRequest/IsCurrentUserHasOpenTask resolve true); "Tasks.Empty" returns none.
    public Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds)
    {
        if (mockUtil.HasMockFeature("Tasks.Empty"))
        {
            return Task.FromResult<List<TaskExistResultDto>?>([]);
        }

        var result = taskTypeIds.Select(taskTypeId => new TaskExistResultDto
        {
            TaskTypeId = taskTypeId,   // TODO: dummy data
            IsTaskInProgress = true,   // TODO: dummy data
            UserId = 5,                // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<TaskExistResultDto>?>(result);
    }
}
