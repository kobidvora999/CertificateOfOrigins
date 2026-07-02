using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using System.Diagnostics.CodeAnalysis;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

[ExcludeFromCodeCoverage]
public class TasksMockProxy : ITasksProxy
{
    // Returns hardcoded dummy data — used while the real Tasks service endpoint is unavailable.
    public Task<List<IsTaskExistResultDto>?> IsTaskExist(IsTaskExistFilterDto filter)
    {
        return Task.FromResult<List<IsTaskExistResultDto>?>(new List<IsTaskExistResultDto>());
    }
}
