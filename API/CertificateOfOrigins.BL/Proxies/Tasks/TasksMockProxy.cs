using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Proxy.Rest;
using System.Diagnostics.CodeAnalysis;

namespace CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksMockProxy(IProxyMockUtil mockUtil) : ITasksProxy, IMockProxy
{
    // Default = one in-progress task per requested type, owned by user 5 (matches the CC-USER-ID used in local
    // testing, so IsCurrentUserHandleRequest/IsCurrentUserHasOpenTask resolve true); "Tasks.Empty" returns none.
    public Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds, bool isTaskInProgress = false)
    {
        if (mockUtil.HasMockFeature("Tasks.Empty"))
        {
            return Task.FromResult<List<TaskExistResultDto>?>([]);
        }

        // "Tasks.ClosedOnly" reproduces the state that broke the importer reminder: a task of the requested type
        // exists but is CLOSED. A caller that counts rows sees "a task exists"; one that respects
        // IsTaskInProgress sees none. Without this flag no mock could tell the two behaviours apart.
        var inProgress = !mockUtil.HasMockFeature("Tasks.ClosedOnly");
        var result = taskTypeIds.Select(taskTypeId => new TaskExistResultDto
        {
            TaskTypeId = taskTypeId,   // TODO: dummy data
            IsTaskInProgress = inProgress,
            UserId = 5,                // TODO: dummy data
        }).ToList();
        return Task.FromResult<List<TaskExistResultDto>?>(result);
    }

    // Legacy IsTaskExistsOnEntity means "an OPEN task exists", so the mock honours Tasks.ClosedOnly here too.
    public Task<bool> IsTaskExistsOnEntity(int entityId, int entityTypeId, int taskTypeId)
    {
        var exists = !mockUtil.HasMockFeature("Tasks.Empty") && !mockUtil.HasMockFeature("Tasks.ClosedOnly");
        return Task.FromResult(exists);
    }

    // Default = an assessor (user 5) handles the lead document, so the mismatch task is assigned; feature
    // "Tasks.NoAssessor" returns none so the HasWarnings event is raised without an assignment.
    public Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter)
    {
        return Task.FromResult<int?>(mockUtil.HasMockFeature("Tasks.NoAssessor") ? null : 5); // TODO: dummy data
    }
}
