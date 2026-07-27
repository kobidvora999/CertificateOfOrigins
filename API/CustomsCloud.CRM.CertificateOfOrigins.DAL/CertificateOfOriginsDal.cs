using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
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

    public async Task<ExportDocumentAuthenticationRequest?> GetExportDocumentAuthenticationRequestById(int id)
    {
        // Single request by id + its three child collections (mirrors the legacy Single + 3x LoadProperty).
        // TEMPORARY: the entity has 35 columns but the platform MaxCountExceededInterceptor errors at >=30 result
        // columns. Until a CertificateOfOrigins entry is added to InfrastructureCore's InterceptorList (then use
        // .Include(...) + .ExcludeInterceptor("<hash>") with the full column set), we project to 29 columns and
        // drop 6 fields: State, CreateDate, CreateUserId, UpdateDate, UpdateUserId, OrganizationUnitId.
        var result = await ReadOnlyContext.ExportDocumentAuthenticationRequests
            .Where(r => r.Id == id)
            .Select(r => new ExportDocumentAuthenticationRequest
            {
                Id = r.Id,
                TypeId = r.TypeId,
                Title = r.Title,
                TimeStamp = r.TimeStamp,
                CustomerId = r.CustomerId,
                AuthenticationDocumentTypeId = r.AuthenticationDocumentTypeId,
                ExporterCustomerId = r.ExporterCustomerId,
                StatusId = r.StatusId,
                CountryId = r.CountryId,
                CustomsHouseAddress = r.CustomsHouseAddress,
                VendorId = r.VendorId,
                AuthenticationRequestArrivalDate = r.AuthenticationRequestArrivalDate,
                AuthenticationRequestedByName = r.AuthenticationRequestedByName,
                AuthenticationRequestedByEmail = r.AuthenticationRequestedByEmail,
                AuthenticationRequestedByPhone = r.AuthenticationRequestedByPhone,
                AuthenticationRequestNotes = r.AuthenticationRequestNotes,
                ExportLeadDocumentId = r.ExportLeadDocumentId,
                DocumentId = r.DocumentId,
                MainDocumentTitle = r.MainDocumentTitle,
                LastDeliveryDate = r.LastDeliveryDate,
                DeliveryMethodId = r.DeliveryMethodId,
                InvoiceNumbers = r.InvoiceNumbers,
                DetailedDecision = r.DetailedDecision,
                ReferenceNumber = r.ReferenceNumber,
                CommentForCustomsHouseLetter = r.CommentForCustomsHouseLetter,
                TotalDocuments = r.TotalDocuments,
                TotalInvoices = r.TotalInvoices,
                DocumentDate = r.DocumentDate,
                InvoiceDate = r.InvoiceDate,
                CustomsItems = r.CustomsItems.ToList(),
                LeadDocuments = r.LeadDocuments.ToList(),
                ManufacturingAreas = r.ManufacturingAreas.ToList(),
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        // dbo.GetCertificateOfOriginByID — a single certificate's full graph (7 result sets), composed in the
        // DbContext extension. Milestone user display-names are NULL from the SP (cross-service JOIN removed) and
        // are enriched in the BL via IUserProxy.
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateOfOriginID", certificateOfOriginId, DbType.Int32);
        var result = await ReadOnlyContext.GetCertificateOfOriginById(parameters);
        return result;
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
