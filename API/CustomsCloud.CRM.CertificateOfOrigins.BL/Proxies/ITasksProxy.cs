using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Proxies;

public interface ITasksProxy
{
    Task<List<IsTaskExistResultDto>?> IsTaskExist(IsTaskExistFilterDto filter);
}
