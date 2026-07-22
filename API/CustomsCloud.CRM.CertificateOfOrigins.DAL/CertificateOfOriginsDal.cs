using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
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

    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters)
    {
        // dbo.GetCertificateOfOriginsByFilter — dynamic-SQL search; exporter/agent titles return NULL from the
        // SP (customer JOINs removed) and are enriched in the BL via the Customers proxy.
        var result = await ReadOnlyContext.GetCertificateOfOriginsByFilter(parameters);
        return result.ToList();
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var isProhibited = await ReadOnlyContext.VerificationProhibitedImporters
            .AnyAsync(c => c.CustomerId == importerId);
        return isProhibited ? null : importerId;
    }
}
