using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginDal : IBaseDal
{
    Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter);

    Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId);

    Task<List<CertificateMilestoneRowDto>> GetCertificateMilestoneRows(string? certificateTitle);

    Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter);
}
