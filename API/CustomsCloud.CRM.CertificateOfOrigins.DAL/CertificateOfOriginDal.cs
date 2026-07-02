using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public class CertificateOfOriginDal(IServiceProvider serviceProvider)
    : BaseDal<CertificateOfOriginDbContext, CertificateOfOriginDbReadOnlyContext>(serviceProvider), ICertificateOfOriginDal
{
    public async Task<List<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(CertificateOfOriginFilterDto filter)
    {
        var query = ReadOnlyContext.CertificateOfOrigins
            .Join(ReadOnlyContext.CertificateOfOriginStatusCodes,
                f => f.CertificateOfOriginStatusId,
                s => s.Id,
                (f, s) => new { Certificate = f, Status = s })
            .Where(x => x.Certificate.State == 1);

        if (filter.CertificateNumber != null)
        {
            query = query.Where(x => x.Certificate.CertificateNumber.Contains(filter.CertificateNumber));
        }

        if (filter.CertificateOfOriginStatusId.HasValue)
        {
            query = query.Where(x => x.Certificate.CertificateOfOriginStatusId == filter.CertificateOfOriginStatusId.Value);
        }

        if (filter.CertificateOfOriginTypeId.HasValue)
        {
            query = query.Where(x => x.Certificate.TypeId == filter.CertificateOfOriginTypeId.Value);
        }

        if (filter.ExporterCustomerId.HasValue)
        {
            query = query.Where(x => x.Certificate.CustomerId == filter.ExporterCustomerId.Value);
        }

        if (filter.CustomsAgentId.HasValue)
        {
            query = query.Where(x => x.Certificate.CreateCustomerId == filter.CustomsAgentId.Value);
        }

        if (filter.CustomsHouseId.HasValue)
        {
            query = query.Where(x => x.Certificate.OrganizationUnitId == filter.CustomsHouseId.Value);
        }

        if (filter.DestinationCountry.HasValue)
        {
            query = query.Where(x => x.Certificate.DestinationCountry == filter.DestinationCountry.Value);
        }

        if (filter.FromIssuingDate.HasValue)
        {
            var fromIssuingDate = filter.FromIssuingDate.Value.Date;
            query = query.Where(x => x.Certificate.IssuingDate >= fromIssuingDate);
        }

        if (filter.ToIssuingDate.HasValue)
        {
            var toIssuingDate = filter.ToIssuingDate.Value.Date.AddDays(1);
            query = query.Where(x => x.Certificate.IssuingDate < toIssuingDate);
        }

        if (filter.FromRequestDate.HasValue)
        {
            var fromRequestDate = filter.FromRequestDate.Value.Date;
            query = query.Where(x => x.Certificate.CreateDate >= fromRequestDate);
        }

        if (filter.ToRequestDate.HasValue)
        {
            var toRequestDate = filter.ToRequestDate.Value.Date.AddDays(1);
            query = query.Where(x => x.Certificate.CreateDate < toRequestDate);
        }

        if (filter.RequestReasonId.HasValue)
        {
            query = query.Where(x => x.Certificate.RequestReasonCode == filter.RequestReasonId.Value);
        }

        if (filter.ExportDeclarationId.HasValue)
        {
            query = query.Where(x => x.Certificate.LeadDocumentId == filter.ExportDeclarationId.Value);
        }

        if (filter.ExportDeclarationNum != null)
        {
            query = query.Where(x => x.Certificate.ExportDeclarationNumber == filter.ExportDeclarationNum);
        }

        if (filter.VersionNumber.HasValue)
        {
            query = query.Where(x => x.Certificate.VersionNumber == filter.VersionNumber.Value);
        }

        if (filter.IsLastVersion.HasValue)
        {
            query = query.Where(x => x.Certificate.IsLastVersion == filter.IsLastVersion.Value);
        }

        var result = await query
            .OrderByDescending(x => x.Certificate.CreateDate)
            .Take(200) // legacy: TOP (shared.ufn_GetMaxRows())
            .Select(x => new CertificateOfOriginResultDto
            {
                Id = x.Certificate.Id,
                CertificateNumber = x.Certificate.CertificateNumber,
                Name = x.Status.Name,
                CustomesAgentId = x.Certificate.CreateCustomerId,
                ExporterId = x.Certificate.CustomerId,
                ExportDeclarationNumber = x.Certificate.ExportDeclarationNumber,
                VersionNumber = x.Certificate.VersionNumber,
                OrganizationUnitId = x.Certificate.OrganizationUnitId,
                RequestReasonCode = x.Certificate.RequestReasonCode,
                IssuingDate = x.Certificate.IssuingDate,
                LeadDocumentId = x.Certificate.LeadDocumentId
            })
            .ToListAsync();
        return result;
    }

    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(int certificateOfOriginId)
    {
        var certificate = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => c.Id == certificateOfOriginId)
            .Select(c => new CertificateOfOriginDto
            {
                Id = c.Id,
                TypeId = c.TypeId,
                Title = c.Title,
                State = c.State,
                CreateDate = c.CreateDate,
                CreateUserId = c.CreateUserId,
                UpdateDate = c.UpdateDate,
                UpdateUserId = c.UpdateUserId,
                OrganizationUnitId = c.OrganizationUnitId,
                CustomerId = c.CustomerId,
                CreateCustomerId = c.CreateCustomerId,
                UpdateCustomerId = c.UpdateCustomerId,
                LeadDocumentId = c.LeadDocumentId,
                CertificateIdToCancel = c.CertificateIdToCancel,
                CertificateNumber = c.CertificateNumber,
                CertificateOfOriginStatusId = c.CertificateOfOriginStatusId,
                DestinationCountry = c.DestinationCountry,
                FeedbackRemark = c.FeedbackRemark,
                InternalApplication = c.InternalApplication,
                IssuingDate = c.IssuingDate,
                RejectCancelReason = c.RejectCancelReason,
                ReplacementReason = c.ReplacementReason,
                RequestReasonCode = c.RequestReasonCode,
                ExportDeclarationNumber = c.ExportDeclarationNumber,
                CertificateToReplaceInImport = c.CertificateToReplaceInImport,
                Guid = c.Guid,
                QrCodePath = c.QrCodePath,
                IsAttachedList = c.IsAttachedList,
                InSufficentworkingInd = c.InSufficentworkingInd,
                InsufficentWorkingText = c.InsufficentWorkingText,
                ApproveUserId = c.ApproveUserId,
                VersionNumber = c.VersionNumber,
                IsLastVersion = c.IsLastVersion,
                IsInPublishingProcess = c.IsInPublishingProcess
            })
            .FirstOrDefaultAsync();
        if (certificate == null)
        {
            return null;
        }

        certificate.CertificateOfOriginVsDeclarationError = await ReadOnlyContext.CertificateOfOriginVsDeclarationErrors
            .Where(e => e.CertificateOfOriginId == certificateOfOriginId)
            .Select(e => new CertificateOfOriginVsDeclarationErrorDto
            {
                Id = e.Id,
                CertificateOfOriginId = e.CertificateOfOriginId,
                ErrorText = e.ErrorText,
                State = e.State
            })
            .ToListAsync();

        // legacy SP loads the full CertificateDetailsTypeCode enum table and matches in memory
        var detailsTypeCodes = await ReadOnlyContext.CertificateDetailsTypeCodes
            .Select(t => new CertificateDetailsTypeCodeEnumDto
            {
                Id = t.Id,
                Name = t.Name,
                State = t.State,
                Description = t.Description,
                EnglishName = t.EnglishName,
                Enumeration = t.Enumeration,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Comment = t.Comment,
                DetailTypeFormat = t.DetailTypeFormat,
                DataTypeId = t.DataTypeId
            })
            .ToListAsync();

        var details = await ReadOnlyContext.CertificateOfOriginDetails
            .Where(d => d.CertificateOfOriginId == certificateOfOriginId)
            .Select(d => new CertificateOfOriginDetailsDto
            {
                Id = d.Id,
                CertificateOfOriginId = d.CertificateOfOriginId,
                CertificateDetailsTypeCodeId = d.CertificateDetailsTypeCodeId,
                Value = d.Value,
                DisplayedValue = d.DisplayedValue
            })
            .ToListAsync();
        foreach (var detail in details)
        {
            detail.CertificateDetailsTypeCode = detailsTypeCodes.FirstOrDefault(t => t.Id == detail.CertificateDetailsTypeCodeId);
        }

        certificate.CertificateOfOriginDetails = details;

        var invoices = await ReadOnlyContext.CertificateOfOriginInvoiceDetails
            .Where(i => i.CertificateOfOriginId == certificateOfOriginId)
            .Select(i => new CertificateOfOriginInvoiceDetailDto
            {
                Id = i.Id,
                CertificateOfOriginId = i.CertificateOfOriginId,
                CurrencyTypeId = i.CurrencyTypeId,
                InvoiceAmount = i.InvoiceAmount,
                InvoiceDate = i.InvoiceDate,
                InvoiceGoodsDescription = i.InvoiceGoodsDescription,
                InvoiceNumber = i.InvoiceNumber,
                IsToPrint = i.IsToPrint
            })
            .ToListAsync();

        var items = await ReadOnlyContext.CertificateOfOriginItemDetails
            .Join(ReadOnlyContext.CertificateOfOriginInvoiceDetails.Where(i => i.CertificateOfOriginId == certificateOfOriginId),
                d => d.CertificateOfOriginInvoiceDetailId,
                i => i.Id,
                (d, i) => d)
            .Select(d => new CertificateOfOriginItemDetailDto
            {
                Id = d.Id,
                PackingTypeId = d.PackingTypeId,
                CustomsItemId = d.CustomsItemId,
                GrossWeight = d.GrossWeight,
                CertificateOfOriginInvoiceDetailId = d.CertificateOfOriginInvoiceDetailId,
                ItemGoodsDescription = d.ItemGoodsDescription,
                MarksAndNumbers = d.MarksAndNumbers,
                MeasurementUnitId = d.MeasurementUnitId,
                OriginCriterionId = d.OriginCriterionId,
                Quantity = d.Quantity,
                RowNum = d.RowNum,
                FullClassification = d.FullClassification,
                ContainerIsoCode = d.ContainerIsoCode
            })
            .ToListAsync();
        foreach (var invoice in invoices)
        {
            invoice.CertificateOfOriginItemDetail = items.Where(d => d.CertificateOfOriginInvoiceDetailId == invoice.Id).ToList();
        }

        certificate.CertificateOfOriginInvoiceDetail = invoices;
        return certificate;
    }

    public async Task<List<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(ImportAuthenticationRequestFilterDto filter)
    {
        var query = ReadOnlyContext.ImportAuthenticationRequests
            .Join(ReadOnlyContext.PrefernceDocumentTypes,
                r => r.PreferenceDocumentTypeId,
                p => p.Id,
                (r, p) => new { Request = r, PreferenceDocumentType = p })
            .GroupJoin(ReadOnlyContext.ImportAuthenticationFileDetails,
                x => x.Request.AuthenticationFileId,
                f => f.Id,
                (x, files) => new { x.Request, x.PreferenceDocumentType, Files = files })
            .SelectMany(x => x.Files.DefaultIfEmpty(), (x, file) => new { x.Request, x.PreferenceDocumentType, File = file });

        if (filter.FromRequestDate.HasValue)
        {
            var fromRequestDate = filter.FromRequestDate.Value.Date;
            query = query.Where(x => x.Request.CreateDate >= fromRequestDate);
        }

        if (filter.ToRequestDate.HasValue)
        {
            var toRequestDate = filter.ToRequestDate.Value.Date.AddDays(1);
            query = query.Where(x => x.Request.CreateDate < toRequestDate);
        }

        if (filter.PrefernceDocumentType.HasValue)
        {
            query = query.Where(x => x.Request.PreferenceDocumentTypeId == filter.PrefernceDocumentType.Value);
        }

        if (filter.GoodsOrigionCountry.HasValue)
        {
            query = query.Where(x => x.Request.OriginCountryId == filter.GoodsOrigionCountry.Value);
        }

        if (filter.IssuingCountry.HasValue)
        {
            query = query.Where(x => x.Request.IssuingCountryId == filter.IssuingCountry.Value);
        }

        if (filter.ImportCountry.HasValue)
        {
            query = query.Where(x => x.Request.ImportCountryId == filter.ImportCountry.Value);
        }

        if (filter.ImporterId.HasValue)
        {
            query = query.Where(x => x.Request.ImporterId == filter.ImporterId.Value);
        }

        if (filter.CustomsHouseId.HasValue)
        {
            query = query.Where(x => x.Request.OrganizationUnitId == filter.CustomsHouseId.Value);
        }

        if (filter.RequestReason.HasValue)
        {
            query = query.Where(x => x.Request.RequestCircumstancesId == filter.RequestReason.Value);
        }

        if (filter.LeadDocumentId.HasValue)
        {
            query = query.Where(x => x.Request.LeadDocumentId == filter.LeadDocumentId.Value);
        }

        if (filter.VendorId.HasValue)
        {
            query = query.Where(x => x.Request.VendorId == filter.VendorId.Value);
        }

        if (filter.DecisionId.HasValue)
        {
            query = query.Where(x => x.Request.DecisionId == filter.DecisionId.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(x => x.Request.CustomerId == filter.CustomerId.Value);
        }

        if (filter.DocumentId.HasValue)
        {
            query = query.Where(x => x.Request.DocumentId == filter.DocumentId.Value);
        }

        if (filter.InvoiceNumber != null)
        {
            // legacy: full-text CONTAINS with comma-separated terms OR-ed together
            var invoiceTerms = filter.InvoiceNumber.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            query = query.Where(x => invoiceTerms.Any(term => x.Request.InvoiceNumber != null && x.Request.InvoiceNumber.Contains(term)));
        }

        if (filter.DocumentNumber != null)
        {
            query = query.Where(x => x.Request.DocumentNumber != null && x.Request.DocumentNumber.Contains(filter.DocumentNumber));
        }

        if (filter.AuthenticationFileId.HasValue)
        {
            query = query.Where(x => x.Request.AuthenticationFileId == filter.AuthenticationFileId.Value);
        }

        if (filter.CreateUserId.HasValue)
        {
            query = query.Where(x => x.Request.CreateUserId == filter.CreateUserId.Value);
        }

        var result = await query
            .OrderByDescending(x => x.Request.CreateDate)
            .Take(200) // legacy: TOP (shared.ufn_GetMaxRows())
            .Select(x => new GetImportAuthenticationRequestResultDto
            {
                DocumentId = x.Request.DocumentId,
                PreferenceDocumentTypeId = x.PreferenceDocumentType.Name,
                AuthenticationFileId = x.Request.AuthenticationFileId,
                CreateDate = x.Request.CreateDate,
                IssuingCountryIdNum = x.Request.IssuingCountryId,
                OrganizationUnitIdNum = x.Request.OrganizationUnitId,
                ResponseNameEmail = x.Request.ResponseNameEmail,
                LeadDocumentId = x.Request.LeadDocumentId,
                CustomerId = x.Request.ImporterId,
                VendorId = x.Request.VendorId,
                DecisionId = x.Request.DecisionId,
                AuthenticationFileStatusId = (int?)x.File!.AuthenticationFileStatusId
            })
            .ToListAsync();
        return result;
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
        var result = await Context.ImportAuthenticationRequests
            .Where(r => documentIds.Contains(r.DocumentId) && r.AuthenticationFileId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.AuthenticationFileId, authenticationFileId));
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

    public async Task<List<ImportAuthenticationRequestDto>> GetRequestsByAuthenticationFileId(int fileId)
    {
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => r.AuthenticationFileId == fileId)
            .Select(r => new ImportAuthenticationRequestDto
            {
                DocumentId = r.DocumentId,
                CreateDate = r.CreateDate,
                CreateUserId = r.CreateUserId,
                UpdateDate = r.UpdateDate,
                UpdateUserId = r.UpdateUserId,
                AuthenticationFileId = r.AuthenticationFileId,
                AuthenticationRequestDate = r.AuthenticationRequestDate,
                CirumstanceDetails = r.CirumstanceDetails,
                CollateralId = r.CollateralId,
                DecisionCircumstences = r.DecisionCircumstences,
                DecisionId = r.DecisionId,
                LeadDocumentId = r.LeadDocumentId,
                DocumentIssuingDate = r.DocumentIssuingDate,
                ImportCountryId = r.ImportCountryId,
                IssuingCountryId = r.IssuingCountryId,
                ItemDetailId = r.ItemDetailId,
                Number = r.Number,
                IsOldIndication = r.IsOldIndication,
                OriginCountryId = r.OriginCountryId,
                PreferenceDocumentTypeId = r.PreferenceDocumentTypeId,
                Remarks = r.Remarks,
                RequestCircumstancesId = r.RequestCircumstancesId,
                UserResponseId = r.UserResponseId,
                ResponseNameEmail = r.ResponseNameEmail,
                ResponsePhoneNum = r.ResponsePhoneNum,
                OrganizationUnitId = r.OrganizationUnitId,
                UserId = r.UserId,
                VendorId = r.VendorId,
                VendorName = r.VendorName,
                OrganizationUnitTypeId = r.OrganizationUnitTypeId,
                DocumentNumber = r.DocumentNumber,
                CustomerId = r.CustomerId,
                ImporterId = r.ImporterId,
                InvoiceNumber = r.InvoiceNumber,
                InvoiceGoodsItemTaxDifference = r.InvoiceGoodsItemTaxDifference,
                AllInvoiceGoodsItemTaxDifference = r.AllInvoiceGoodsItemTaxDifference
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsItemDetailDto>> GetItemDetailsByRequestIds(List<int> requestIds)
    {
        var result = await ReadOnlyContext.CertificateOfOriginsItemDetails
            .Where(i => i.ImportAuthenticationRequestId != null && requestIds.Contains(i.ImportAuthenticationRequestId.Value))
            .Select(i => new CertificateOfOriginsItemDetailDto
            {
                Id = i.Id,
                ImportAuthenticationRequestId = i.ImportAuthenticationRequestId,
                CustomItemId = i.CustomItemId
            })
            .ToListAsync();
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
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => r.DocumentId == documentId)
            .Select(r => new ImportAuthenticationRequestDto
            {
                DocumentId = r.DocumentId,
                CreateDate = r.CreateDate,
                CreateUserId = r.CreateUserId,
                UpdateDate = r.UpdateDate,
                UpdateUserId = r.UpdateUserId,
                AuthenticationFileId = r.AuthenticationFileId,
                AuthenticationRequestDate = r.AuthenticationRequestDate,
                CirumstanceDetails = r.CirumstanceDetails,
                CollateralId = r.CollateralId,
                DecisionCircumstences = r.DecisionCircumstences,
                DecisionId = r.DecisionId,
                LeadDocumentId = r.LeadDocumentId,
                DocumentIssuingDate = r.DocumentIssuingDate,
                ImportCountryId = r.ImportCountryId,
                IssuingCountryId = r.IssuingCountryId,
                ItemDetailId = r.ItemDetailId,
                Number = r.Number,
                IsOldIndication = r.IsOldIndication,
                OriginCountryId = r.OriginCountryId,
                PreferenceDocumentTypeId = r.PreferenceDocumentTypeId,
                Remarks = r.Remarks,
                RequestCircumstancesId = r.RequestCircumstancesId,
                UserResponseId = r.UserResponseId,
                ResponseNameEmail = r.ResponseNameEmail,
                ResponsePhoneNum = r.ResponsePhoneNum,
                OrganizationUnitId = r.OrganizationUnitId,
                UserId = r.UserId,
                VendorId = r.VendorId,
                VendorName = r.VendorName,
                OrganizationUnitTypeId = r.OrganizationUnitTypeId,
                DocumentNumber = r.DocumentNumber,
                CustomerId = r.CustomerId,
                ImporterId = r.ImporterId,
                InvoiceNumber = r.InvoiceNumber,
                InvoiceGoodsItemTaxDifference = r.InvoiceGoodsItemTaxDifference,
                AllInvoiceGoodsItemTaxDifference = r.AllInvoiceGoodsItemTaxDifference
            })
            .FirstOrDefaultAsync();
        return result;
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
        var result = await ReadOnlyContext.ImportAuthenticationRequests
            .Where(r => leadDocumentIds.Contains(r.LeadDocumentId))
            .Join(ReadOnlyContext.PrefernceDocumentTypes,
                r => r.PreferenceDocumentTypeId,
                p => p.Id,
                (r, p) => new { Request = r, PreferenceDocumentType = p })
            .Join(ReadOnlyContext.CertificateOfOriginsDecisions,
                x => x.Request.DecisionId,
                d => (int?)d.Id,
                (x, d) => new { x.Request, x.PreferenceDocumentType, Decision = d })
            .Select(x => new ImportAuthenticationRequestByLeadDocumentDto
            {
                LeadDocumentId = x.Request.LeadDocumentId,
                DocumentId = x.Request.DocumentId,
                AuthenticationFileId = x.Request.AuthenticationFileId,
                PreferenceDocumentTypeId = x.Request.PreferenceDocumentTypeId,
                PreferenceDocumentTypeName = x.PreferenceDocumentType.Name,
                CreateDate = x.Request.CreateDate,
                AuthenticationFileStatusId = ReadOnlyContext.ImportAuthenticationFileDetails
                    .Where(f => f.Id == x.Request.AuthenticationFileId)
                    .Select(f => (int?)f.AuthenticationFileStatusId)
                    .FirstOrDefault(),
                AuthenticationFileStatusName = ReadOnlyContext.ImportAuthenticationFileDetails
                    .Where(f => f.Id == x.Request.AuthenticationFileId)
                    .Join(ReadOnlyContext.CertificateOfOriginsAuthenticationFileStatuses,
                        f => f.AuthenticationFileStatusId,
                        s => s.Id,
                        (f, s) => s.Name)
                    .FirstOrDefault(),
                DecisionId = x.Decision.Id,
                DecisionName = x.Decision.Name,
                ImportCountryId = x.Request.ImportCountryId,
                OrganizationUnitId = x.Request.OrganizationUnitId,
                CollateralId = x.Request.CollateralId,
                IsCollateralExists = x.Request.CollateralId != null
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateMilestoneRowDto>> GetCertificateMilestoneRows(string? certificateTitle)
    {
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Join(ReadOnlyContext.CertificateOfOriginStatusCodes,
                c => c.CertificateOfOriginStatusId,
                s => s.Id,
                (c, s) => c)
            .Where(c => c.Title == certificateTitle)
            .Where(c => (c.RejectCancelReason != null && c.IssuingDate == null && c.ApproveUserId == null && c.UpdateUserId > 1000)
                || (c.ApproveUserId != null && c.CertificateOfOriginStatusId != 4)
                || (c.ApproveUserId != null && c.IssuingDate != null && c.CertificateOfOriginStatusId == 4))
            .OrderBy(c => c.Id)
            .Select(c => new CertificateMilestoneRowDto
            {
                VersionNumber = c.VersionNumber,
                ActionName = c.RejectCancelReason != null && c.IssuingDate == null && c.ApproveUserId == null && c.UpdateUserId > 1000
                    ? "נדחתה"
                    : c.ApproveUserId != null && c.IssuingDate != null && c.CertificateOfOriginStatusId == 4
                        ? "בוטלה לאחר פרסום"
                        : c.ApproveUserId != null && c.CertificateOfOriginStatusId != 4
                            ? "אושרה"
                            : null,
                CreateDate = c.UpdateDate,
                RejectReason = c.RejectCancelReason ?? string.Empty,
                UserId = c.CertificateOfOriginStatusId == 8 ? c.ApproveUserId : c.UpdateUserId
            })
            .ToListAsync();
        return result;
    }
}
