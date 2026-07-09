using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ITasksProxy
{
    Task<List<IsTaskExistResultDto>?> IsTaskExist(IsTaskExistFilterDto filter);

    /// <summary>
    /// The user id currently handling the entity's tasks (with task unification), or null when none.
    /// </summary>
    Task<int?> GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilterDto filter);
}
