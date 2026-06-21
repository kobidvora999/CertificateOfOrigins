using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(object? parameters)
    {
        var result = await ReadOnlyContext.GetAuthenticationRequestById(parameters);
        return result;
    }

    public async Task<List<CertificateOfOriginsDecisionDto>?> GetCertificateOfOriginsDecisions()
    {
        var result = await ReadOnlyContext.CertificateOfOriginsDecisions
            .ExcludeInterceptor("T7e0Y38X2y")
            .Where(d => d.State != 99)
            .Select(d => new CertificateOfOriginsDecisionDto
            {
                Id = d.Id,
                Name = d.Name,
                State = d.State,
                Description = d.Description,
                EnglishName = d.EnglishName,
                Enumeration = d.Enumeration,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                IsAutomatic = d.IsAutomatic,
                IsForCoordinator = d.IsForCoordinator,
                IsForClaliMakorWorker = d.IsForClaliMakorWorker
            })
            .ToListAsync();
        return result;
    }

    public async Task<bool> IsVendorCountry(int issuingCountryId)
    {
        var result = await ReadOnlyContext.CertificateOfOriginSupplierDeliveryCountryConfigs
            .ExcludeInterceptor("T7e0Y38X2y")
            .AnyAsync(c => c.ConutryId == issuingCountryId && c.State != 99);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(object? parameters)
    {
        var result = await ReadOnlyContext.CheckIfExistsAdditionalRequestsForVendor(parameters);
        return result;
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var isProhibited = await ReadOnlyContext.VerificationProhibitedImporters
            .AnyAsync(c => c.CustomerId == importerId);
        return isProhibited ? null : importerId;
    }
}
