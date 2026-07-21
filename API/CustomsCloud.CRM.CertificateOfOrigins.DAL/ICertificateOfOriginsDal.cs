using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public interface ICertificateOfOriginsDal : IBaseDal
{
    Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber);
}
