using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginDal : IBaseDal
{
    Task<List<CertificateOfOriginResultDto>?> GetCertificateOfOriginsByFilter(object? parameters);
    Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters);
}
