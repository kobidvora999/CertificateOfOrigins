using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.Proxies;

public interface ITasksProxy
{
    // Legacy: ITasksExternalProxy.IsTaskExist(IsTaskExistFilter). One row per matching task, carrying that task's
    // own IsTaskInProgress and UserID — callers that need the per-task UserID (the IsCurrentUserHandle* flags)
    // genuinely need the list, which is why this is not a bool.
    //
    // isTaskInProgress mirrors IsTaskExistFilter.IsTaskInProgress, which the port had dropped. ⚠️ Sending it is
    // NOT sufficient today: Infrastructure.usp_Tasks_IsTaskExist accepts the parameter but its status-filtering
    // branches are commented out, so it is ignored server-side and every status comes back. Callers that need
    // open-only semantics must ALSO filter the result on IsTaskInProgress. It is sent anyway so the contract
    // matches the legacy one and starts working the day the SP is restored.
    Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds, bool isTaskInProgress = false);

    // Legacy: ITasksExternalProxy.IsTaskExistsOnEntity(VirtualEntity, taskTypeID) — a SEPARATE operation from
    // IsTaskExist, returning bool. TasksBL pins IsTaskInProgress = true inside it, so "exists" here has always
    // meant "an OPEN task exists". The port had collapsed it into IsTaskExist and lost that, which is why a
    // closed task began suppressing events that legacy still raised.
    Task<bool> IsTaskExistsOnEntity(int entityId, int entityTypeId, int taskTypeId);

    // Legacy: ITasksExternalServiceAdapter.GetLatestUserHandlingEntityTasksWithTaskUnification(filter).UserID
    // (UpdateCertificateOfOrigins) — the user (assessor) latest handling the entity's tasks, or null when none.
    Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter);
}
