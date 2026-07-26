using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

    public async Task<List<GetImportAuthenticationRequestResultDto>> GetImportAuthenticationRequestByFilter(object? parameters)
    {
        // dbo.GetImportAuthenticationRequestByFilter — dynamic-SQL search; importer/vendor/country names return
        // NULL from the SP (cross-service JOINs removed) and are enriched in the BL (proxies + Country lookup).
        var result = await ReadOnlyContext.GetImportAuthenticationRequestByFilter(parameters);
        return result.ToList();
    }

    public async Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters)
    {
        // dbo.ExportDocumentAuthenticationRequestSearch — dynamic-SQL search; country/customer names return NULL
        // from the SP (cross-service JOINs removed) and are enriched in the BL (Country lookup + Customers proxy).
        var result = await ReadOnlyContext.GetExportDocumentAuthenticationRequestSearch(parameters);
        return result.ToList();
    }

    public async Task<List<GetAuthenticationRequestByLeadDocumentResultDto>> GetAuthenticationRequestByLeadDocumentIDs(object? parameters)
    {
        // dbo.GetAuthenticationRequestByLeadDocumentID — TVP-filtered (Shared.IntArray) query; country/org-unit
        // names return NULL from the SP (cross-service JOINs removed) and are enriched in the BL via lookups.
        var result = await ReadOnlyContext.GetAuthenticationRequestByLeadDocumentID(parameters);
        return result.ToList();
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var isProhibited = await ReadOnlyContext.VerificationProhibitedImporters
            .AnyAsync(c => c.CustomerId == importerId);
        return isProhibited ? null : importerId;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@VendorID", vendorId, DbType.Int32);
        var result = await ReadOnlyContext.CheckIfExistsAdditionalRequestsForVendor(parameters);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@ImporterID", importerId, DbType.Int32);
        parameters.Add("@VendorID", vendorId, DbType.Int32);
        parameters.Add("@CustomerID", customerId, DbType.Int32);
        parameters.Add("@CountryID", countryId, DbType.Int32);
        var result = await ReadOnlyContext.CheckIfExistsAdditionalRequestsForImporter(parameters);
        return result;
    }
}
