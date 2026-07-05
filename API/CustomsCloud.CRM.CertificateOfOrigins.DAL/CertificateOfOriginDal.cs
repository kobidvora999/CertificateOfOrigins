using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public class CertificateOfOriginDal(IServiceProvider serviceProvider)
    : BaseDal<CertificateOfOriginDbContext, CertificateOfOriginDbReadOnlyContext>(serviceProvider), ICertificateOfOriginDal
{
    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters)
    {
        // dbo.GetCertificateOfOriginsByFilter - dynamic-SQL search; exporter/agent titles return NULL
        // from the SP and are enriched in the BL via the Customers proxy
        var result = await ReadOnlyContext.GetCertificateOfOriginsByFilter(parameters);
        return result.ToList();
    }

    public async Task<(CertificateOfOriginDto? Certificate, List<CertificateMilestoneRowDto> MilestoneRows)> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        // dbo.GetCertificateOfOriginByID - 7 result sets (certificate graph + milestone rows);
        // milestone user names are enriched in the BL via the Users proxy (UserMng_User not replicated)
        var parameters = new DynamicParameters();
        parameters.Add("@CertificateOfOriginID", certificateOfOriginId, DbType.Int32);
        var result = await ReadOnlyContext.GetCertificateOfOriginByID(parameters);
        return result;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(object? parameters)
    {
        // dbo.GetImportAuthenticationRequestByFilter - dynamic-SQL search; cross-service name columns
        // (IssuingCountryID/OrganizationUnitID/VendorName/ImporterName/LeadDocumentTitle) return NULL
        // from the SP and are enriched in the BL (FillAuthenticationRequestNames)
        var result = await ReadOnlyContext.GetAuthenticationRequestByFilter(parameters);
        return result.ToList();
    }

    public async Task<List<int>> GetRequestedDocumentIdsByLeadDocumentId(int leadDocumentId)
    {
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => r.LeadDocumentId == leadDocumentId)
            .Select(r => r.DocumentId)
            .ToListAsync();
        return result;
    }

    public async Task<List<int>> GetDocumentIdsUsedByOtherLeadDocuments(List<int> documentIds, int leadDocumentId)
    {
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.LeadDocumentId != leadDocumentId)
            .Select(r => r.DocumentId)
            .ToListAsync();
        return result;
    }

    public async Task<ImportAuthenticationRequest?> GetFirstRequestWithAuthenticationFile(List<int> documentIds)
    {
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.AuthenticationFileId != null)
            .Select(r => new ImportAuthenticationRequest
            {
                DocumentId = r.DocumentId,
                AuthenticationFileId = r.AuthenticationFileId
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<ImportAuthenticationFileDetails> CreateAuthenticationFile(ImportAuthenticationFileDetails file)
    {
        Context.ImportAuthenticationFileDetails.Add(file);
        await Context.SaveChangesAsync();
        return file;
    }

    public async Task<int> UpdateRequestsAuthenticationFileId(List<int> documentIds, int authenticationFileId)
    {
        // dbo.UpdateImportAuthenticationRequest — the legacy SP owns the update logic
        // (UPDATE ... SET AuthenticationFileID WHERE DocumentID IN (TVP) AND AuthenticationFileID IS NULL)
        var table = new DataTable();
        table.Columns.Add("val", typeof(int));
        foreach (var id in documentIds)
        {
            table.Rows.Add(id);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@ImportAuthenticationRequestsID", table.AsTableValuedParameter("Shared.IntArray"));
        parameters.Add("@AuthenticationFileID", authenticationFileId, DbType.Int32);
        var result = await Context.UpdateImportAuthenticationRequest(parameters);
        return result;
    }

    public async Task<ImportAuthenticationFileDetailsDto?> GetAuthenticationFileById(int fileId)
    {
        var result = await ReadOnlyContext.ImportAuthenticationFileDetails
            .Where(f => f.Id == fileId)
            .Select(f => new ImportAuthenticationFileDetailsDto
            {
                Id = f.Id,
                State = f.State,
                CreateDate = f.CreateDate,
                CreateUserId = f.CreateUserId,
                UpdateDate = f.UpdateDate,
                UpdateUserId = f.UpdateUserId,
                AuthenticationFileStatusId = f.AuthenticationFileStatusId,
                Notes = f.Notes,
                PostalAdress = f.PostalAdress,
                DeliveryMethodId = f.DeliveryMethodId,
                EmailAdress = f.EmailAdress,
                ReminderMethodId = f.ReminderMethodId,
                RequestCountryId = f.RequestCountryId,
                UserId = f.UserId,
                UserNameIssuingLetter = f.UserNameIssuingLetter,
                LastDelivery = f.LastDelivery,
                ImporterContactingReasonId = f.ImporterContactingReasonId,
                FirstProvideContactDate = f.FirstProvideContactDate
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<(ImportAuthenticationFileDetailsDto? File, List<ImportAuthenticationRequestDto> Requests, List<CertificateOfOriginsItemDetailDto> ItemDetails)> GetAuthenticationFileDetailsAndRequests(int fileId)
    {
        // dbo.GetImportAuthenticationFileDetailsAndRequests - 4 result sets in one round-trip
        // (file header / requests / documents-empty / item details); documents come from the Documents microservice
        var parameters = new DynamicParameters();
        parameters.Add("@FileID", fileId, DbType.Int32);
        var result = await ReadOnlyContext.GetImportAuthenticationFileDetailsAndRequests(parameters);
        return result;
    }

    public async Task<List<CertificateOfOriginsDecisionDto>> GetAllDecisions()
    {
        var result = await ReadOnlyContext.CertificateOfOriginsDecisions
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

    public async Task<List<CertificateOfOriginsAuthenticationFileStatusDto>> GetAllAuthenticationFileStatuses()
    {
        var result = await ReadOnlyContext.CertificateOfOriginsAuthenticationFileStatuses
            .Select(s => new CertificateOfOriginsAuthenticationFileStatusDto
            {
                Id = s.Id,
                Name = s.Name,
                State = s.State,
                Description = s.Description,
                EnglishName = s.EnglishName,
                Enumeration = s.Enumeration,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsAutomatic = s.IsAutomatic
            })
            .ToListAsync();
        return result;
    }

    public async Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(int documentId)
    {
        // dbo.GetImportAuthenticationRequestById — 3 result sets: request / item details / documents (empty — Documents microservice)
        var parameters = new DynamicParameters();
        parameters.Add("@DocumentID", documentId, DbType.Int32);
        var (request, itemDetails) = await ReadOnlyContext.GetImportAuthenticationRequestById(parameters);
        if (request != null)
        {
            request.ItemDetails = itemDetails;
        }

        return request;
    }

    public async Task<bool> IsVendorCountry(int issuingCountryId)
    {
        var result = await ReadOnlyContext.CertificateOfOriginSupplierDeliveryCountryConfigs
            .ExcludeInterceptor("T7e0Y38X2y")
            .AnyAsync(c => c.ConutryId == issuingCountryId && c.State != 99);
        return result;
    }

    public async Task<List<ImportAuthenticationRequestByLeadDocumentDto>> GetAuthenticationRequestsByLeadDocumentIds(List<int> leadDocumentIds)
    {
        // dbo.GetAuthenticationRequestByLeadDocumentID — name columns (LeadDocumentTitle/ImportCountryName/
        // OrganizationUnitName) return NULL from the SP and are enriched in the BL via lookup/proxy
        var table = new DataTable();
        table.Columns.Add("val", typeof(int));
        foreach (var id in leadDocumentIds)
        {
            table.Rows.Add(id);
        }

        var parameters = new DynamicParameters();
        parameters.Add("@LeadDocumentIDs", table.AsTableValuedParameter("Shared.IntArray"));
        var result = await ReadOnlyContext.GetAuthenticationRequestByLeadDocumentID(parameters);
        return result.ToList();
    }

    public async Task<List<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters)
    {
        // dbo.ExportDocumentAuthenticationRequestSearch - dynamic-SQL search; CountryName/
        // ForeignCustomsHouseName/RequestIssuerName return NULL from the SP (raw CountryId/
        // ExporterCustomerId columns added) and are enriched in the BL via lookup + Customers proxy
        var result = await ReadOnlyContext.GetExportDocumentAuthenticationRequestSearch(parameters);
        return result.ToList();
    }

    public async Task<ExportDocumentAuthenticationRequestDto?> GetExportDocumentAuthenticationRequestById(int id)
    {
        var request = await ReadOnlyContext.ExportDocumentAuthenticationRequests
            .Where(e => e.Id == id)
            .Select(e => new ExportDocumentAuthenticationRequestDto
            {
                Id = e.Id,
                TypeId = e.TypeId,
                Title = e.Title,
                State = e.State,
                CreateDate = e.CreateDate,
                CreateUserId = e.CreateUserId,
                UpdateDate = e.UpdateDate,
                UpdateUserId = e.UpdateUserId,
                OrganizationUnitId = e.OrganizationUnitId,
                CustomerId = e.CustomerId,
                AuthenticationDocumentTypeId = e.AuthenticationDocumentTypeId,
                ExporterCustomerId = e.ExporterCustomerId,
                StatusId = e.StatusId,
                CountryId = e.CountryId,
                CustomsHouseAddress = e.CustomsHouseAddress,
                VendorId = e.VendorId,
                AuthenticationRequestArrivalDate = e.AuthenticationRequestArrivalDate,
                AuthenticationRequestedByName = e.AuthenticationRequestedByName,
                AuthenticationRequestedByEmail = e.AuthenticationRequestedByEmail,
                AuthenticationRequestedByPhone = e.AuthenticationRequestedByPhone,
                AuthenticationRequestNotes = e.AuthenticationRequestNotes,
                ExportLeadDocumentId = e.ExportLeadDocumentId,
                DocumentId = e.DocumentId,
                MainDocumentTitle = e.MainDocumentTitle,
                LastDeliveryDate = e.LastDeliveryDate,
                DeliveryMethodId = e.DeliveryMethodId,
                InvoiceNumbers = e.InvoiceNumbers,
                DetailedDecision = e.DetailedDecision,
                ReferenceNumber = e.ReferenceNumber,
                CommentForCustomsHouseLetter = e.CommentForCustomsHouseLetter,
                TotalDocuments = e.TotalDocuments,
                TotalInvoices = e.TotalInvoices,
                DocumentDate = e.DocumentDate,
                InvoiceDate = e.InvoiceDate
            })
            .FirstOrDefaultAsync();
        if (request == null)
        {
            return null;
        }

        request.CustomsItemToExportDocumentAuthenticationRequest = await ReadOnlyContext.CustomsItemToExportDocumentAuthenticationRequests
            .Where(c => c.ExportDocumentAuthenticationRequestId == id)
            .Select(c => new CustomsItemToExportDocumentAuthenticationRequestDto
            {
                Id = c.Id,
                ExportDocumentAuthenticationRequestId = c.ExportDocumentAuthenticationRequestId,
                CustomsItemId = c.CustomsItemId
            })
            .ToListAsync();

        request.ExportDocumentAuthenticationRequestLeadDocument = await ReadOnlyContext.ExportDocumentAuthenticationRequestLeadDocuments
            .Where(l => l.ExportRequestId == id)
            .Select(l => new ExportDocumentAuthenticationRequestLeadDocumentDto
            {
                Id = l.Id,
                ExportRequestId = l.ExportRequestId,
                LeadDocumentId = l.LeadDocumentId,
                LeadDocumentTitle = l.LeadDocumentTitle
            })
            .ToListAsync();

        request.ExportAuthenticationRequestManufacturingArea = await ReadOnlyContext.ExportAuthenticationRequestManufacturingAreas
            .Where(m => m.ExportAuthenticationRequestId == id)
            .Select(m => new ExportAuthenticationRequestManufacturingAreaDto
            {
                Id = m.Id,
                ExportAuthenticationRequestId = m.ExportAuthenticationRequestId,
                ManufacturingArea = m.ManufacturingArea,
                ManufacturingZipcode = m.ManufacturingZipcode
            })
            .ToListAsync();

        return request;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(int importerId, int? vendorId, int? customerId, int countryId, int daysForLastDelivery)
    {
        // dbo.CheckIfExistsAdditionalRequestsForImporter — the SP computes isVendor internally
        // from the local SupplierDeliveryCountryConfig table
        var parameters = new DynamicParameters();
        parameters.Add("@ImporterID", importerId, DbType.Int32);
        parameters.Add("@VendorID", vendorId, DbType.Int32);
        parameters.Add("@CustomerID", customerId, DbType.Int32);
        parameters.Add("@CountryID", countryId, DbType.Int32);
        parameters.Add("@DaysForLastDelivery", daysForLastDelivery, DbType.Int32);
        var result = await ReadOnlyContext.CheckIfExistsAdditionalRequestsForImporter(parameters);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(int vendorId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@VendorID", vendorId, DbType.Int32);
        var result = await ReadOnlyContext.CheckIfExistsAdditionalRequestsForVendor(parameters);
        return result;
    }

    public async Task<int?> CheckImporterOfImportAuthentication(int importerId)
    {
        var isProhibited = await ReadOnlyContext.VerificationProhibitedImporters
            .AnyAsync(c => c.CustomerId == importerId);
        return isProhibited ? null : importerId;
    }

    public async Task<ImportAuthenticationRequestDto> SaveImportAuthenticationRequest(ImportAuthenticationRequestDto dto)
    {
        var entity = await Context.ImportAuthenticationRequests.FindAsync(dto.DocumentId) ?? new ImportAuthenticationRequest { DocumentId = dto.DocumentId };
        if (entity.CreateDate == default)
        {
            Context.ImportAuthenticationRequests.Add(entity);
        }

        entity.CreateDate = dto.CreateDate.DateTime;
        entity.CreateUserId = dto.CreateUserId;
        entity.UpdateDate = dto.UpdateDate.DateTime;
        entity.UpdateUserId = dto.UpdateUserId;
        entity.AuthenticationFileId = dto.AuthenticationFileId;
        entity.AuthenticationRequestDate = dto.AuthenticationRequestDate.DateTime;
        entity.CirumstanceDetails = dto.CirumstanceDetails;
        entity.CollateralId = dto.CollateralId;
        entity.DecisionCircumstences = dto.DecisionCircumstences;
        entity.DecisionId = dto.DecisionId;
        entity.LeadDocumentId = dto.LeadDocumentId;
        entity.DocumentIssuingDate = dto.DocumentIssuingDate.DateTime;
        entity.ImportCountryId = dto.ImportCountryId;
        entity.IssuingCountryId = dto.IssuingCountryId;
        entity.ItemDetailId = dto.ItemDetailId;
        entity.Number = dto.Number;
        entity.IsOldIndication = dto.IsOldIndication;
        entity.OriginCountryId = dto.OriginCountryId;
        entity.PreferenceDocumentTypeId = dto.PreferenceDocumentTypeId;
        entity.Remarks = dto.Remarks ?? string.Empty;
        entity.RequestCircumstancesId = dto.RequestCircumstancesId;
        entity.UserResponseId = dto.UserResponseId;
        entity.ResponseNameEmail = dto.ResponseNameEmail;
        entity.ResponsePhoneNum = dto.ResponsePhoneNum;
        entity.OrganizationUnitId = dto.OrganizationUnitId;
        entity.UserId = dto.UserId;
        entity.VendorId = dto.VendorId;
        entity.VendorName = dto.VendorName;
        entity.OrganizationUnitTypeId = dto.OrganizationUnitTypeId;
        entity.DocumentNumber = dto.DocumentNumber;
        entity.CustomerId = dto.CustomerId;
        entity.ImporterId = dto.ImporterId;
        entity.LastDeliveryForImporter = dto.LastDeliveryForImporter == null ? null : dto.LastDeliveryForImporter.Value.DateTime;
        entity.InvoiceNumber = dto.InvoiceNumber;
        entity.InvoiceGoodsItemTaxDifference = dto.InvoiceGoodsItemTaxDifference;
        entity.AllInvoiceGoodsItemTaxDifference = dto.AllInvoiceGoodsItemTaxDifference;
        await Context.SaveChangesAsync();
        return dto;
    }

    public async Task SaveAuthenticationFileDetails(ImportAuthenticationFileDetailsDto dto)
    {
        var entity = await Context.ImportAuthenticationFileDetails.FindAsync(dto.Id) ?? new ImportAuthenticationFileDetails();
        if (entity.Id == 0)
        {
            Context.ImportAuthenticationFileDetails.Add(entity);
        }

        entity.State = dto.State;
        entity.CreateDate = dto.CreateDate.DateTime;
        entity.CreateUserId = dto.CreateUserId;
        entity.UpdateDate = dto.UpdateDate.DateTime;
        entity.UpdateUserId = dto.UpdateUserId;
        entity.AuthenticationFileStatusId = dto.AuthenticationFileStatusId;
        entity.Notes = dto.Notes;
        entity.PostalAdress = dto.PostalAdress ?? string.Empty;
        entity.DeliveryMethodId = dto.DeliveryMethodId;
        entity.EmailAdress = dto.EmailAdress;
        entity.ReminderMethodId = dto.ReminderMethodId;
        entity.RequestCountryId = dto.RequestCountryId;
        entity.UserId = dto.UserId;
        entity.UserNameIssuingLetter = dto.UserNameIssuingLetter ?? string.Empty;
        entity.LastDelivery = dto.LastDelivery == null ? null : dto.LastDelivery.Value.DateTime;
        entity.ImporterContactingReasonId = dto.ImporterContactingReasonId;
        entity.FirstProvideContactDate = dto.FirstProvideContactDate == null ? null : dto.FirstProvideContactDate.Value.DateTime;
        await Context.SaveChangesAsync();
    }

    public async Task<int> TouchRequestsUpdateDateByFileId(int fileId, DateTime updateDate)
    {
        var result = await Context.ImportAuthenticationRequests
            .Where(r => r.AuthenticationFileId == fileId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UpdateDate, updateDate));
        return result;
    }

    public async Task<int?> GetCertificateOfOriginIdByNumber(string certificateNumber)
    {
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => c.CertificateNumber == certificateNumber)
            .OrderByDescending(c => c.CreateDate)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<string?> GetCertificateOfOriginTypeCodeName(int typeCodeId)
    {
        var result = await ReadOnlyContext.CertificateOfOriginTypeCodes
            .Where(t => t.Id == typeCodeId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync();
        return result;
    }
}
