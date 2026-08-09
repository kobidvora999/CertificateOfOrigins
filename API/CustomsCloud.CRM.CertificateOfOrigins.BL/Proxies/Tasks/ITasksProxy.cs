using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ITasksProxy
{
    // Legacy: ITasksExternalProxy.IsTaskExist(IsTaskExistFilter). The tasks of the given types on an entity, from
    // the Tasks microservice.
    Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds);

    // Legacy: ITasksExternalServiceAdapter.GetLatestUserHandlingEntityTasksWithTaskUnification(filter).UserID
    // (UpdateCertificateOfOrigins) — the user (assessor) latest handling the entity's tasks, or null when none.
    Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter);
}
