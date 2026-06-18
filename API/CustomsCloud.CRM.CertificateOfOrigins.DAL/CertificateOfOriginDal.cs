using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public class CertificateOfOriginDal(IServiceProvider serviceProvider)
    : BaseDal<CertificateOfOriginDbContext, CertificateOfOriginDbReadOnlyContext>(serviceProvider), ICertificateOfOriginDal
{
    public async Task<List<CertificateOfOriginResultDto>?> GetCertificateOfOriginsByFilter(object? parameters)
    {
        var result = await ReadOnlyContext.GetCertificateOfOriginsByFilter(parameters);
        return result.ToList();
    }

    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters)
    {
        var result = await ReadOnlyContext.GetCertificateOfOriginById(parameters);
        return result;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>?> GetAuthenticationRequestByFilter(object? parameters)
    {
        var result = await ReadOnlyContext.GetImportAuthenticationRequestByFilter(parameters);
        return result.ToList();
    }
}
