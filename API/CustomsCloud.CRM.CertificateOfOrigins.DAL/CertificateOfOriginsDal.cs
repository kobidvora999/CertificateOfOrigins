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

    public async Task<CertificateOfOriginsImportAuthenticationRequest?> GetImportAuthenticationRequestById(int documentId)
    {
        // GetAuthenticationRequestByID SP result-set #1 (main row), local table only — the legacy CRP.DealFile
        // LEFT JOIN (LeadDocumentSubmissionDate) is dropped (cross-service, deferred). Projected to the needed
        // columns (< 30) to stay under the platform column-count interceptor. Missing → null (404 in the BL).
        var result = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.DocumentId == documentId)
            .Select(r => new CertificateOfOriginsImportAuthenticationRequest
            {
                DocumentId = r.DocumentId,
                CreateDate = r.CreateDate,
                AuthenticationFileId = r.AuthenticationFileId,
                AuthenticationRequestDate = r.AuthenticationRequestDate,
                CollateralId = r.CollateralId,
                DecisionId = r.DecisionId,
                LeadDocumentId = r.LeadDocumentId,
                DocumentIssuingDate = r.DocumentIssuingDate,
                ImportCountryId = r.ImportCountryId,
                IssuingCountryId = r.IssuingCountryId,
                Number = r.Number,
                OriginCountryId = r.OriginCountryId,
                PreferenceDocumentTypeId = r.PreferenceDocumentTypeId,
                ResponseNameEmail = r.ResponseNameEmail,
                OrganizationUnitId = r.OrganizationUnitId,
                VendorId = r.VendorId,
                VendorName = r.VendorName,
                CustomerId = r.CustomerId,
                ImporterId = r.ImporterId,
                LastDeliveryForImporter = r.LastDeliveryForImporter,
                InvoiceNumber = r.InvoiceNumber,
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsItemDetails>> GetItemDetailsByRequestId(int documentId)
    {
        // GetAuthenticationRequestByID SP result-set #2 — the item lines of the request.
        var result = await ReadOnlyContext.CertificateOfOriginsItemDetails
            .Where(i => i.ImportAuthenticationRequestId == documentId)
            .Select(i => new CertificateOfOriginsItemDetails
            {
                Id = i.Id,
                ImportAuthenticationRequestId = i.ImportAuthenticationRequestId,
                CustomItemId = i.CustomItemId,
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsDecision>> GetAllDecisions()
    {
        // Legacy GetQuery<CertificateOfOriginsDecision>().ToList() — the full decision lookup table.
        var result = await ReadOnlyContext.CertificateOfOriginsDecisions
            .Select(d => new CertificateOfOriginsDecision
            {
                Id = d.Id,
                Name = d.Name,
                State = d.State,
                Description = d.Description,
                EnglishName = d.EnglishName,
                Enumeration = d.Enumeration,
                StartDate = d.StartDate,
            })
            .ToListAsync();
        return result;
    }

    public async Task<bool> IsSupplierDeliveryCountry(int countryId)
    {
        // Legacy IsVendor: GetIdByCode<...>("ConutryID", countryId) > 0 — true when the issuing country has an active
        // supplier-delivery config row (soft-delete filter State != 99 per repo convention).
        var result = await ReadOnlyContext.CertificateOfOriginsSupplierDeliveryCountryConfigs
            .AnyAsync(c => c.ConutryId == countryId && c.State != 99);
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

    public async Task<CertificateOfOriginWebQueryDto?> GetCertificateOfOriginDataForWebQuery(object? parameters)
    {
        // dbo.GetCertificateOfOriginDataForWebQuery — the public-portal certificate-verification query (5 result
        // sets), composed in the DbContext extension. DocumentId is NULL from the SP (cross-service Docs JOIN
        // removed) and is left unresolved in the BL (TODO(blocking)).
        var result = await ReadOnlyContext.GetCertificateOfOriginDataForWebQuery(parameters);
        return result;
    }

    public async Task<List<int>> GetImportAuthenticationRequestDocumentIdsByLeadDocumentId(int leadDocumentId)
    {
        // The DocumentIDs of the import-authentication requests already registered under this lead document
        // (legacy: GetQuery<...ImportAuthenticationRequest>().Where(LeadDocumentID == x).Select(DocumentID)).
        var result = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.LeadDocumentId == leadDocumentId)
            .Select(r => r.DocumentId)
            .ToListAsync();
        return result;
    }

    public async Task<List<int>> GetImportAuthenticationRequestDocumentIdsClaimedByOtherLeadDocuments(List<int> documentIds, int leadDocumentId)
    {
        // Of the given document ids, those already claimed by a DIFFERENT lead document (legacy second query:
        // Where(Ids.Contains(DocumentID) && LeadDocumentID != x).Select(DocumentID)).
        var result = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.LeadDocumentId != leadDocumentId)
            .Select(r => r.DocumentId)
            .ToListAsync();
        return result;
    }

    public async Task<bool> UpdateFileAfterDelivery(int fileId, int authenticationFileStatusId, int deliveryMethodId)
    {
        // Faithful to the legacy UpdateFileAfterDelivery: advance the file's status/delivery-method (computed in the
        // BL from the client-sent values) + stamp LastDelivery/UpdateDate, and touch every child request's UpdateDate.
        // Set-based writes (ExecuteUpdateAsync) — no row loaded, matching the "trust the client" decision.
        var now = DateTimeOffset.Now;
        var today = new DateTimeOffset(now.Date, now.Offset);

        await Context.CertificateOfOriginsImportAuthenticationFileDetails
            .Where(f => f.Id == fileId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.AuthenticationFileStatusId, authenticationFileStatusId)
                .SetProperty(f => f.DeliveryMethodId, deliveryMethodId)
                .SetProperty(f => f.LastDelivery, today)
                .SetProperty(f => f.UpdateDate, today));

        await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.AuthenticationFileId == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UpdateDate, now));

        return true;
    }

    public async Task<bool> UpdateRequestDecisionAfterDelivery(int documentId, int decisionId)
    {
        // Faithful to the legacy importer flow: stamp the request's DecisionID + LastDeliveryForImporter + UpdateDate.
        // (The parent file + all its child requests' UpdateDate are handled separately by UpdateFileAfterDelivery,
        // which — matching the legacy loop — overrides this request's UpdateDate to "now".) Set-based, no row loaded.
        var today = new DateTimeOffset(DateTimeOffset.Now.Date, DateTimeOffset.Now.Offset);
        await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.DocumentId == documentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.DecisionId, decisionId)
                .SetProperty(r => r.LastDeliveryForImporter, today)
                .SetProperty(r => r.UpdateDate, today));
        return true;
    }

    public async Task<(int DocumentId, int FileId)?> GetFirstRequestAlreadyLinkedToFile(List<int> documentIds)
    {
        // The first of the given requests that already belongs to a file — drives the FileExistForRequest validation.
        var row = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.AuthenticationFileId != null)
            .Select(r => new { r.DocumentId, r.AuthenticationFileId })
            .FirstOrDefaultAsync();
        return row is null ? null : (row.DocumentId, row.AuthenticationFileId!.Value);
    }

    public async Task<int> InsertAuthenticationFile(CertificateOfOriginsImportAuthenticationFileDetails file)
    {
        Context.CertificateOfOriginsImportAuthenticationFileDetails.Add(file);
        await Context.SaveChangesAsync();
        return file.Id;
    }

    public async Task<bool> LinkRequestsToAuthenticationFile(List<int> documentIds, int fileId)
    {
        // Faithful to usp_CertificateOfOrigins_UpdateImportAuthenticationRequest: link only requests not already
        // attached to a file (set-based ExecuteUpdate, replacing the legacy SP + Shared.IntArray TVP — developer
        // decision 2026-07-30).
        await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.AuthenticationFileId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.AuthenticationFileId, fileId));
        return true;
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
