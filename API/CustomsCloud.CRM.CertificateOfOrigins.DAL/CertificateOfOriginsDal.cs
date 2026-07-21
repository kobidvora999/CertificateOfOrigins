using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public class CertificateOfOriginsDal(IServiceProvider serviceProvider)
    : BaseDal<CertificateOfOriginsDbContext, CertificateOfOriginsDbReadOnlyContext>(serviceProvider), ICertificateOfOriginsDal
{
    public async Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber)
    {
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => c.CertificateNumber == certificateNumber)
            .OrderByDescending(c => c.CreateDate)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
        return result;
    }
}
