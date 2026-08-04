using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ITasksProxy
{
    // Legacy: ITasksExternalProxy.IsTaskExist(IsTaskExistFilter). The tasks of the given types on an entity, from
    // the Tasks microservice.
    Task<List<TaskExistResultDto>?> IsTaskExist(int entityId, int entityTypeId, List<int> taskTypeIds);
}
