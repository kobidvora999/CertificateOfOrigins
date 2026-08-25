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

    public async Task<CertificateOfOrigin?> GetLatestCertificateByNumberForFeedback(string certificateNumber)
    {
        // GetPC_MSG2280_2281 (GetRequestStatus / CertificateCancellation): the latest certificate with this number,
        // projected to the fields the feedback response + the cancel write need.
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => c.CertificateNumber == certificateNumber)
            .OrderByDescending(c => c.Id)
            .Select(c => new CertificateOfOrigin
            {
                Id = c.Id,
                TypeId = c.TypeId,
                CustomerId = c.CustomerId,
                CertificateNumber = c.CertificateNumber,
                CertificateOfOriginStatusId = c.CertificateOfOriginStatusId,
                RequestReasonCode = c.RequestReasonCode,
                InternalApplication = c.InternalApplication,
                FeedbackRemark = c.FeedbackRemark,
                RejectCancelReason = c.RejectCancelReason,
                IssuingDate = c.IssuingDate,
                OrganizationUnitId = c.OrganizationUnitId,
                Guid = c.Guid,
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<int> GetNextCertificateOfOriginNumber()
    {
        // GetPC_MSG2280_2281 create branch: the next certificate-number numerator (legacy
        // GetCertificateNumber → dbo.GetCertificateOfOriginNumber → NEXT VALUE FOR the CRM sequence). Scalar SP.
        var connection = ReadOnlyContext.Database.GetDbConnection();
        var command = new CommandDefinition("dbo.GetCertificateOfOriginNumber", commandType: CommandType.StoredProcedure);
        var numerator = await connection.ExecuteScalarAsync<int>(command);
        return numerator;
    }

    public async Task<OriginCriterion?> GetOriginCriterion(string originCriterionCode, int certificateOfOriginTypeCodeId)
    {
        // GetPC_MSG2280_2281 create branch: resolve an origin-criterion code scoped to a certificate type to its row
        // (legacy SystemTablesUtil.GetTablesSync<OriginCriterion> with a code + certificate-type predicate). Local C-table.
        var result = await ReadOnlyContext.OriginCriterions
            .Where(o => o.OriginCriterionCode == originCriterionCode && o.CertificateOfOriginTypeCodeId == certificateOfOriginTypeCodeId)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<DetailsPerCertificate>> GetDetailsPerCertificate(int certificateOfOriginTypeCodeId)
    {
        // GetPC_MSG2280_2281 field-validation engine: the field catalogue for a certificate type — which detail fields
        // are relevant and each field's constraint (Mandatory/Optional/Condition). Legacy
        // CertificateOfOriginsUtil.GetDetailsPerCertificate.
        var result = await ReadOnlyContext.DetailsPerCertificates
            .Where(d => d.CertificateOfOriginTypeCodeId == certificateOfOriginTypeCodeId)
            .ToListAsync();
        return result;
    }

    public async Task CancelCertificateFromMessage(int id, string rejectCancelReason, int userId)
    {
        // GetPC_MSG2280_2281 CertificateCancellation: set the certificate to Cancelled with the cancel-from-message
        // reason. Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.CertificateOfOriginStatusId, (int)ECertificateOfOriginStatus.Cancelled)
                .SetProperty(c => c.RejectCancelReason, rejectCancelReason)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
    }

    public async Task<CertificateOfOrigin?> GetLatestCertificateByNumber(string certificateNumber)
    {
        // SaveCertificateOfOrigin: the latest existing certificate with the same number (the one a new instance
        // supersedes). Projected to the columns the cancel-previous-version logic needs.
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => c.CertificateNumber == certificateNumber)
            .OrderByDescending(c => c.Id)
            .Select(c => new CertificateOfOrigin
            {
                Id = c.Id,
                CertificateOfOriginStatusId = c.CertificateOfOriginStatusId,
                VersionNumber = c.VersionNumber,
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<int> SaveCertificateOfOrigin(CertificateOfOrigin entity, List<CertificateOfOriginDetails> details, int userId)
    {
        return await SaveCertificateOfOrigin(entity, details, [], userId);
    }

    public async Task<int> SaveCertificateOfOrigin(CertificateOfOrigin entity, List<CertificateOfOriginDetails> details, List<CertificateOfOriginInvoiceDetail> invoices, int userId)
    {
        // Upsert the certificate (Id == 0 → insert with fresh audit, else update via the round-tripped TimeStamp for
        // concurrency, preserving the immutable audit columns), then DIFF-MERGE its detail rows by surrogate id, then
        // (incoming-message create branch) DIFF-MERGE its invoice rows + each invoice's item rows.
        var now = DateTime.Now;
        if (entity.Id == 0)
        {
            entity.CreateDate = now;
            entity.CreateUserId = userId;
            entity.UpdateDate = now;
            entity.UpdateUserId = userId;
            Context.CertificateOfOrigins.Add(entity);
        }
        else
        {
            entity.UpdateDate = now;
            entity.UpdateUserId = userId;
            Context.CertificateOfOrigins.Update(entity);

            // The round-tripped DTO does not carry the immutable audit columns, so Update would overwrite them with
            // defaults (CreateDate = 0001-01-01 → SqlDateTime overflow). Keep the existing DB values.
            Context.Entry(entity).Property(e => e.CreateDate).IsModified = false;
            Context.Entry(entity).Property(e => e.CreateUserId).IsModified = false;
        }

        await Context.SaveChangesAsync();

        foreach (var detail in details)
        {
            detail.CertificateOfOriginId = entity.Id;
        }

        await MergeChildrenAsync(
            details,
            Context.Set<CertificateOfOriginDetails>().Where(d => d.CertificateOfOriginId == entity.Id),
            detail => detail.Id);
        await Context.SaveChangesAsync();

        await SaveInvoiceDetails(entity.Id, invoices);

        return entity.Id;
    }

    // Diff-merge the certificate's invoice rows, then each invoice's item rows. Invoices are keyed to the certificate;
    // items are keyed to their invoice's surrogate id (assigned after the invoice save for freshly-inserted invoices).
    // An EMPTY list means "the caller did not manage invoices" (the SPA save path) — do NOT touch the existing rows;
    // only the incoming-message create branch supplies invoices, and it always supplies the full set. (Diff-merging an
    // empty list would delete every existing invoice/item — a data-loss bug for the SPA update path.)
    private async Task SaveInvoiceDetails(int certificateId, List<CertificateOfOriginInvoiceDetail> invoices)
    {
        if (invoices.Count == 0)
        {
            return;
        }

        foreach (var invoice in invoices)
        {
            invoice.CertificateOfOriginId = certificateId;
        }

        await MergeChildrenAsync(
            invoices,
            Context.Set<CertificateOfOriginInvoiceDetail>().Where(i => i.CertificateOfOriginId == certificateId),
            invoice => invoice.Id);
        await Context.SaveChangesAsync();

        foreach (var invoice in invoices)
        {
            var items = invoice.CertificateOfOriginItemDetail;
            foreach (var item in items)
            {
                item.CertificateOfOriginInvoiceDetailId = invoice.Id;
            }

            await MergeChildrenAsync(
                items,
                Context.Set<CertificateOfOriginItemDetail>().Where(it => it.CertificateOfOriginInvoiceDetailId == invoice.Id),
                item => item.Id);
        }

        await Context.SaveChangesAsync();
    }

    public async Task<List<CertificateOfOrigin>> GetCertificatesByIds(List<int> ids)
    {
        // UpdateCertificateOfOrigins: the certificates to reconcile against the export declaration. Projected to the
        // columns the reconciler reads.
        var result = await ReadOnlyContext.CertificateOfOrigins
            .Where(c => ids.Contains(c.Id))
            .Select(c => new CertificateOfOrigin
            {
                Id = c.Id,
                TypeId = c.TypeId,
                CertificateNumber = c.CertificateNumber,
                CertificateOfOriginStatusId = c.CertificateOfOriginStatusId,
                RequestReasonCode = c.RequestReasonCode,
                LeadDocumentId = c.LeadDocumentId,
                ExportDeclarationNumber = c.ExportDeclarationNumber,
                OrganizationUnitId = c.OrganizationUnitId,
                RejectCancelReason = c.RejectCancelReason,
                CreateDate = c.CreateDate,
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateReconcileInvoiceDto>> GetCertificateInvoiceDetailsByCertificateIds(List<int> certificateIds)
    {
        // UpdateCertificateOfOrigins reconciliation: each certificate invoice + its goods items' customs-item ids (the
        // certificate side of the invoice / goods-item matching against the export declaration).
        var result = await ReadOnlyContext.CertificateOfOriginInvoiceDetails
            .Where(invoice => certificateIds.Contains(invoice.CertificateOfOriginId))
            .Select(invoice => new CertificateReconcileInvoiceDto
            {
                CertificateOfOriginId = invoice.CertificateOfOriginId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomsItemIds = ReadOnlyContext.CertificateOfOriginItemDetails
                    .Where(item => item.CertificateOfOriginInvoiceDetailId == invoice.Id && item.CustomsItemId.HasValue)
                    .Select(item => item.CustomsItemId!.Value)
                    .ToList(),
            })
            .ToListAsync();
        return result;
    }

    public async Task<bool?> GetCertificateTypeIsCustomsItemMandatory(int certificateTypeId)
    {
        // UpdateCertificateOfOrigins reconciliation: whether the certificate type requires the customs-item (6-digit)
        // match (legacy CertificateOfOriginsUtil.GetCertificateTypeCode(...).IsCustomsItemMandatory). Null (no row / bit
        // NULL) means not mandatory.
        var result = await ReadOnlyContext.CertificateOfOriginTypeCodes
            .Where(type => type.Id == certificateTypeId)
            .Select(type => type.IsCustomsItemMandatory)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<CertificateOfOriginTypeCode?> GetCertificateTypeCode(int certificateTypeId)
    {
        // GetPC_MSG2280_2281 create branch: the certificate type's mandatory flags (IsCriterionMandatory /
        // IsCustomsItemMandatory / IsZipcodeMandatory) that drive the invoice/item + zipcode validation (legacy
        // CertificateOfOriginsUtil.GetCertificateTypeCode).
        var result = await ReadOnlyContext.CertificateOfOriginTypeCodes
            .Where(type => type.Id == certificateTypeId)
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginDetails>> GetCertificateDetailsByCertificateIds(List<int> certificateIds)
    {
        // UpdateCertificateOfOrigins reconciliation: the certificates' detail rows (destination / origin country,
        // exporter id, country groups) compared against the export declaration. Projected to the columns the validator
        // reads. No navigation exists on CertificateOfOrigin, so the details are loaded here keyed by certificate id.
        var result = await ReadOnlyContext.CertificateOfOriginDetails
            .Where(d => certificateIds.Contains(d.CertificateOfOriginId))
            .Select(d => new CertificateOfOriginDetails
            {
                Id = d.Id,
                CertificateOfOriginId = d.CertificateOfOriginId,
                CertificateDetailsTypeCodeId = d.CertificateDetailsTypeCodeId,
                Value = d.Value,
            })
            .ToListAsync();
        return result;
    }

    public async Task AddCertificateVsDeclarationErrors(int certificateOfOriginId, List<string> errorTexts)
    {
        // UpdateCertificateOfOrigins: append one mismatch-log row per reconciliation error (append-only).
        if (errorTexts.Count == 0)
        {
            return;
        }

        var rows = errorTexts
            .Select(text => new CertificateOfOriginVsDeclarationError
            {
                CertificateOfOriginId = certificateOfOriginId,
                ErrorText = text,
                State = 1,
            })
            .ToList();
        Context.Set<CertificateOfOriginVsDeclarationError>().AddRange(rows);
        await Context.SaveChangesAsync();
    }

    public async Task UpdateCertificateReconciliation(int id, int statusId, string? exportDeclarationNumber, int? leadDocumentId, string? rejectCancelReason, int userId)
    {
        // UpdateCertificateOfOrigins: stamp the reconciled status + backfilled declaration link (+ reject reason on a
        // mismatch) + update-audit. Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.CertificateOfOriginStatusId, statusId)
                .SetProperty(c => c.ExportDeclarationNumber, exportDeclarationNumber)
                .SetProperty(c => c.LeadDocumentId, leadDocumentId)
                .SetProperty(c => c.RejectCancelReason, rejectCancelReason)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
    }

    public async Task UpdateCertificatePublishingState(int id, DateTime issuingDate, bool isInPublishingProcess, int userId)
    {
        // SaveCertificateOfOrigin (publish): persist the issuing date + the issue-by-worker flag stamped after the main
        // upsert (legacy PrintCertificateOfOriginAndSaveAttachments set these then Save(certificate)). Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IssuingDate, issuingDate)
                .SetProperty(c => c.IsInPublishingProcess, isInPublishingProcess)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
    }

    public async Task UpdateCertificateDeclarationLink(int id, int? leadDocumentId, string? exportDeclarationNumber, int userId)
    {
        // SaveCertificateOfOrigin (LinkLeadDocument): persist the lead-document + declaration-number backfill stamped
        // after the main upsert (LinkLeadDocument needs the new certificate id, so it runs after the save). Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.LeadDocumentId, leadDocumentId)
                .SetProperty(c => c.ExportDeclarationNumber, exportDeclarationNumber)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
    }

    public async Task UpdateCertificateQrCodePath(int id, string? qrCodePath, int userId)
    {
        // SaveCertificateOfOrigin (UploadQrCodeDocument): persist the QR document path resolved AFTER the main upsert.
        // The QR document is uploaded linked to the assigned certificate id, so its resource path is unknown at upsert
        // time and needs its own write here. QrImage + Guid are stamped by the main upsert; only QrCodePath lands here.
        // Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.QrCodePath, qrCodePath)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
    }

    public async Task CancelPreviousCertificate(int id, string rejectCancelReasonSuffix, int userId)
    {
        // SaveCertificateOfOrigin: when a new instance supersedes an existing certificate, cancel the old one
        // (status → Cancelled, append the "update received" reason, drop IsLastVersion). Set-based.
        var now = DateTime.Now;
        await Context.CertificateOfOrigins
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.CertificateOfOriginStatusId, (int)ECertificateOfOriginStatus.Cancelled)
                .SetProperty(c => c.RejectCancelReason, c => (c.RejectCancelReason ?? string.Empty) + rejectCancelReasonSuffix)
                .SetProperty(c => c.IsLastVersion, false)
                .SetProperty(c => c.UpdateDate, now)
                .SetProperty(c => c.UpdateUserId, userId));
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

    public async Task<CertificateOfOriginsImportAuthenticationFileDetails?> GetAuthenticationFileById(int fileId)
    {
        // GetAuthenticationRequestFileByID SP result-set #1 (file header), local table. Missing → null (404 in BL).
        var result = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationFileDetails
            .Where(f => f.Id == fileId)
            .Select(f => new CertificateOfOriginsImportAuthenticationFileDetails
            {
                Id = f.Id,
                State = f.State,
                CreateDate = f.CreateDate,
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
                FirstProvideContactDate = f.FirstProvideContactDate,
            })
            .FirstOrDefaultAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsImportAuthenticationRequest>> GetRequestsByFileId(int fileId)
    {
        // GetAuthenticationRequestFileByID SP result-set #2 (child requests), local table by AuthenticationFileID.
        // Projected to the needed columns (< 30 for the platform interceptor); the CRP.DealFile join +
        // Infrastructure.Tasks_Task OUTER APPLY are resolved in the BL via proxies (LeadDocumentSubmissionDate,
        // IsSendReminderForImporterTaskExists).
        var result = await ReadOnlyContext.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.AuthenticationFileId == fileId)
            .Select(r => new CertificateOfOriginsImportAuthenticationRequest
            {
                DocumentId = r.DocumentId,
                CreateDate = r.CreateDate,
                AuthenticationFileId = r.AuthenticationFileId,
                AuthenticationRequestDate = r.AuthenticationRequestDate,
                DecisionId = r.DecisionId,
                LeadDocumentId = r.LeadDocumentId,
                DocumentIssuingDate = r.DocumentIssuingDate,
                ImportCountryId = r.ImportCountryId,
                IssuingCountryId = r.IssuingCountryId,
                OriginCountryId = r.OriginCountryId,
                PreferenceDocumentTypeId = r.PreferenceDocumentTypeId,
                ResponseNameEmail = r.ResponseNameEmail,
                OrganizationUnitId = r.OrganizationUnitId,
                VendorId = r.VendorId,
                CustomerId = r.CustomerId,
                ImporterId = r.ImporterId,
                LastDeliveryForImporter = r.LastDeliveryForImporter,
                InvoiceNumber = r.InvoiceNumber,
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsAuthenticationFileStatus>> GetAllFileStatuses()
    {
        // Legacy GetQuery<CertificateOfOriginsAuthenticationFileStatus>().ToList() — the full file-status lookup table.
        var result = await ReadOnlyContext.CertificateOfOriginsAuthenticationFileStatuses
            .Select(s => new CertificateOfOriginsAuthenticationFileStatus
            {
                Id = s.Id,
                Name = s.Name,
                State = s.State,
                Description = s.Description,
                EnglishName = s.EnglishName,
                Enumeration = s.Enumeration,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsAutomatic = s.IsAutomatic,
            })
            .ToListAsync();
        return result;
    }

    public async Task<List<CertificateOfOriginsItemDetails>> GetItemDetailsByRequestIds(List<int> requestIds)
    {
        // GetAuthenticationRequestFileByID SP result-set #4 — item lines for all the file's requests (batched).
        var result = await ReadOnlyContext.CertificateOfOriginsItemDetails
            .Where(i => i.ImportAuthenticationRequestId != null && requestIds.Contains(i.ImportAuthenticationRequestId.Value))
            .Select(i => new CertificateOfOriginsItemDetails
            {
                Id = i.Id,
                ImportAuthenticationRequestId = i.ImportAuthenticationRequestId,
                CustomItemId = i.CustomItemId,
            })
            .ToListAsync();
        return result;
    }

    public async Task<bool> SaveImportAuthenticationRequest(SaveImportAuthenticationRequestRequestDto request, int userId)
    {
        // Set-based merge via ExecuteUpdateAsync (the repo's write convention, first used in #22). The Save DTO carries
        // only the round-trip editable fields (a subset of the entity's ~37 columns), so this updates exactly those
        // columns + the update-audit stamp and leaves everything else (CreateDate/CreateUserId, ItemDetailID, the
        // circumstance/remark columns, …) untouched — a genuine merge without fetching the full row (a tracked full-row
        // fetch would also trip the 30-column read interceptor). DocumentID is a non-identity, externally-assigned key
        // — this method only ever edits an existing request; no matching row → false (404 in the BL).
        var now = DateTimeOffset.Now;
        var affected = await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.DocumentId == request.DocumentId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.AuthenticationFileId, request.AuthenticationFileId)
                .SetProperty(r => r.AuthenticationRequestDate, request.AuthenticationRequestDate)
                .SetProperty(r => r.CollateralId, request.CollateralId)
                .SetProperty(r => r.DecisionId, request.DecisionId)
                .SetProperty(r => r.LeadDocumentId, request.LeadDocumentId)
                .SetProperty(r => r.DocumentIssuingDate, request.DocumentIssuingDate)
                .SetProperty(r => r.ImportCountryId, request.ImportCountryId)
                .SetProperty(r => r.IssuingCountryId, request.IssuingCountryId)
                .SetProperty(r => r.Number, request.Number)
                .SetProperty(r => r.OriginCountryId, request.OriginCountryId)
                .SetProperty(r => r.PreferenceDocumentTypeId, request.PreferenceDocumentTypeId)
                .SetProperty(r => r.ResponseNameEmail, request.ResponseNameEmail)
                .SetProperty(r => r.OrganizationUnitId, request.OrganizationUnitId)
                .SetProperty(r => r.VendorId, request.VendorId)
                .SetProperty(r => r.VendorName, request.VendorName)
                .SetProperty(r => r.CustomerId, request.CustomerId)
                .SetProperty(r => r.ImporterId, request.ImporterId)
                .SetProperty(r => r.LastDeliveryForImporter, request.LastDeliveryForImporter)
                .SetProperty(r => r.InvoiceNumber, request.InvoiceNumber)
                .SetProperty(r => r.UserId, request.UserId)
                .SetProperty(r => r.UserResponseId, request.UserResponseId)
                .SetProperty(r => r.UpdateDate, now)
                .SetProperty(r => r.UpdateUserId, userId));

        return affected > 0;
    }

    public async Task<int> SaveExportDocumentAuthenticationRequest(ExportDocumentAuthenticationRequest entity)
    {
        // Upsert the parent (Id == 0 → insert, else update via the round-tripped TimeStamp for concurrency), then
        // DIFF-MERGE its three child collections by surrogate id (developer decision 2026-08-05, revised: reproduce
        // the legacy Self-Tracking-Entity Save — update round-tripped children in place, insert new ones, delete the
        // dropped ones). The children are held aside so the parent Add/Update touches only the parent row.
        var customsItems = entity.CustomsItems;
        var leadDocuments = entity.LeadDocuments;
        var manufacturingAreas = entity.ManufacturingAreas;
        entity.CustomsItems = [];
        entity.LeadDocuments = [];
        entity.ManufacturingAreas = [];

        if (entity.Id == 0)
        {
            Context.ExportDocumentAuthenticationRequests.Add(entity);
        }
        else
        {
            Context.ExportDocumentAuthenticationRequests.Update(entity);

            // The round-tripped DTO does not carry the immutable audit columns, so Update would overwrite them with
            // defaults (CreateDate = 0001-01-01 → SqlDateTime overflow). Keep the existing DB values.
            Context.Entry(entity).Property(e => e.CreateDate).IsModified = false;
            Context.Entry(entity).Property(e => e.CreateUserId).IsModified = false;

            // The read projection (GetById) omits State + OrganizationUnitId (29-column interceptor limit), so the
            // round-tripped DTO carries them as 0 — without this guard, Update would zero both columns on every save
            // of an existing record. Preserve the DB values (parity with the legacy full-entity round-trip).
            Context.Entry(entity).Property(e => e.State).IsModified = false;
            Context.Entry(entity).Property(e => e.OrganizationUnitId).IsModified = false;
        }

        await Context.SaveChangesAsync();

        // Bind the incoming children to the (now known) parent id and detach the nav so the merge touches only the
        // child rows. Ids are preserved (NOT reset) so round-tripped children update in place.
        foreach (var item in customsItems)
        {
            item.ExportDocumentAuthenticationRequestId = entity.Id;
            item.Request = null;
        }

        foreach (var leadDocument in leadDocuments)
        {
            leadDocument.ExportRequestId = entity.Id;
            leadDocument.Request = null;
        }

        foreach (var area in manufacturingAreas)
        {
            area.ExportAuthenticationRequestId = entity.Id;
            area.Request = null;
        }

        await MergeChildrenAsync(
            customsItems,
            Context.Set<CustomsItemToExportDocumentAuthenticationRequest>().Where(c => c.ExportDocumentAuthenticationRequestId == entity.Id),
            item => item.Id);
        await MergeChildrenAsync(
            leadDocuments,
            Context.Set<ExportDocumentAuthenticationRequestLeadDocument>().Where(l => l.ExportRequestId == entity.Id),
            leadDocument => leadDocument.Id);
        await MergeChildrenAsync(
            manufacturingAreas,
            Context.Set<ExportAuthenticationRequestManufacturingArea>().Where(m => m.ExportAuthenticationRequestId == entity.Id),
            area => area.Id);

        await Context.SaveChangesAsync();

        return entity.Id;
    }

    // Diff-merge a child collection against the DB by surrogate id (reproduces the legacy Self-Tracking-Entity Save):
    // delete the rows the client dropped, update the round-tripped rows (Id > 0) in place, and insert the new ones
    // (Id == 0) — preserving unchanged children's ids instead of re-creating every row. Used by
    // SaveExportDocumentAuthenticationRequest.
    private async Task MergeChildrenAsync<TChild>(
        List<TChild> incoming,
        IQueryable<TChild> existingForParent,
        Func<TChild, int> getId)
        where TChild : class
    {
        var keptIds = incoming.Where(child => getId(child) != 0).Select(getId).ToList();

        // Delete existing rows under this parent that the client did not send back (empty keptIds → delete all).
        await existingForParent
            .Where(child => !keptIds.Contains(EF.Property<int>(child, "Id")))
            .ExecuteDeleteAsync();

        var set = Context.Set<TChild>();
        foreach (var child in incoming)
        {
            if (getId(child) == 0)
            {
                set.Add(child);
            }
            else
            {
                set.Update(child);
            }
        }
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

    public async Task<List<GetExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(ExportDocumentAuthenticationRequestSearchFilterDto filter)
    {
        // Faithful LINQ port of dbo.ExportDocumentAuthenticationRequestSearch (a dynamic-filter search — every dynamic
        // clause was just a conditional AND, no procedural logic). The two INNER JOINs to the enum tables both resolve
        // a Name AND exclude rows whose type/status is unmatched; the two IS-NOT-NULL guards preserve the row-membership
        // of the removed cross-service INNER JOINs on the nullable FKs (CountryID / ExporterCustomerID). CustomerID is
        // non-nullable so its INNER JOIN never excludes on null — no guard needed. ExportDeclarationTitle is the
        // legacy OUTER APPLY (first lead-document title by id). CountryName / ForeignCustomsHouseName / RequestIssuerName
        // stay null here and are enriched in the BL (Country lookup + Customers proxy).
        // The two IS-NOT-NULL guards + all conditional filters are on the request row itself (fluent .Where chain,
        // matching the rest of the DAL). The two INNER JOINs (for the names) are applied after — they also exclude
        // rows with an unmatched type/status, exactly like the legacy SP.
        var requests = ReadOnlyContext.ExportDocumentAuthenticationRequests
            .Where(ear => ear.CountryId != null && ear.ExporterCustomerId != null);

        if (filter.CountryId.HasValue)
        {
            requests = requests.Where(ear => ear.CountryId == filter.CountryId);
        }

        if (filter.DocumentTypeId.HasValue)
        {
            requests = requests.Where(ear => ear.AuthenticationDocumentTypeId == filter.DocumentTypeId);
        }

        if (filter.RequestId.HasValue)
        {
            requests = requests.Where(ear => ear.Id == filter.RequestId);
        }

        if (filter.ForeignCustomsHouseId.HasValue)
        {
            requests = requests.Where(ear => ear.CustomerId == filter.ForeignCustomsHouseId);
        }

        if (filter.RequestOpenDateFrom.HasValue)
        {
            requests = requests.Where(ear => ear.CreateDate >= filter.RequestOpenDateFrom);
        }

        if (filter.RequestOpenDateTo.HasValue)
        {
            requests = requests.Where(ear => ear.CreateDate <= filter.RequestOpenDateTo);
        }

        if (filter.ExportAuthenticationDocumentId.HasValue)
        {
            requests = requests.Where(ear => ear.DocumentId == filter.ExportAuthenticationDocumentId);
        }

        var invoiceIdNum = filter.InvoiceIdNum;
        if (invoiceIdNum != null)
        {
            requests = requests.Where(ear => ear.InvoiceNumbers != null && ear.InvoiceNumbers.Contains(invoiceIdNum));
        }

        var mainDocumentTitle = filter.MainDocumentTitle;
        if (mainDocumentTitle != null)
        {
            requests = requests.Where(ear => ear.MainDocumentTitle != null && ear.MainDocumentTitle.Contains(mainDocumentTitle));
        }

        if (filter.ExporterCustomerId.HasValue)
        {
            requests = requests.Where(ear => ear.ExporterCustomerId == filter.ExporterCustomerId);
        }

        if (filter.ExportAuthenticationRequestStatusId.HasValue)
        {
            requests = requests.Where(ear => ear.StatusId == filter.ExportAuthenticationRequestStatusId);
        }

        if (filter.CreateUserId.HasValue)
        {
            requests = requests.Where(ear => ear.CreateUserId == filter.CreateUserId);
        }

        // filter.ExportDeclarationId is a dead SP parameter (declared, never referenced) — intentionally not applied.
        var result = await requests
            .Join(
                ReadOnlyContext.PreferenceDocumentTypes,
                ear => ear.AuthenticationDocumentTypeId,
                dt => dt.Id,
                (ear, dt) => new { ear, DocumentTypeName = dt.Name })
            .Join(
                ReadOnlyContext.ExportAuthenticationRequestStatuses,
                x => x.ear.StatusId,
                ears => (int?)ears.Id,
                (x, ears) => new { x.ear, x.DocumentTypeName, RequestStatusName = ears.Name })
            .OrderBy(x => x.ear.Id)
            .Select(x => new GetExportDocumentAuthenticationRequestSearchResultDto
            {
                RequestId = x.ear.Id,
                CountryId = x.ear.CountryId,
                CustomerId = x.ear.CustomerId,
                DocumentTypeName = x.DocumentTypeName,
                ExportDeclarationTitle = x.ear.LeadDocuments
                    .OrderBy(l => l.Id)
                    .Select(l => l.LeadDocumentTitle)
                    .FirstOrDefault(),
                RequestStatusName = x.RequestStatusName,
                ExporterCustomerId = x.ear.ExporterCustomerId,
                ExportLeadDocumentId = x.ear.ExportLeadDocumentId,
            })
            .ToListAsync();

        return result;
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
        // Verified 2026-08-18 still enforced on InfrastructureCore.DAL 1.10.53 (a 35-column probe threw
        // DbInterceptionException "Result fields count (35) exceeded max eror level of 30"). Keep the workaround
        // until the package exempts this module; re-test after any InfrastructureCore.DAL bump.
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

    public async Task UpdateImportRequestDecision(int documentId, int? decisionId, bool isOldIndication, int userId)
    {
        // SaveAuthenticationRequestFile step 1 (UpdateAndSaveImportAuthenticationRequest): stamp each child request's
        // decision + the recomputed IsOldIndication flag + update-audit. Set-based, no row loaded.
        var now = DateTimeOffset.Now;
        await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.DocumentId == documentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.DecisionId, decisionId)
                .SetProperty(r => r.IsOldIndication, isOldIndication)
                .SetProperty(r => r.UpdateDate, now)
                .SetProperty(r => r.UpdateUserId, userId));
    }

    public async Task<bool> UpdateAuthenticationFile(SaveAuthenticationRequestFileRequestDto file, int userId)
    {
        // SaveAuthenticationRequestFile step 4: persist the file's editable scalar columns + update-audit. Set-based
        // (the repo write convention) — CreateDate/CreateUserId/State/TimeStamp are left untouched. Missing row → false.
        var now = DateTimeOffset.Now;
        var affected = await Context.CertificateOfOriginsImportAuthenticationFileDetails
            .Where(f => f.Id == file.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.AuthenticationFileStatusId, file.AuthenticationFileStatusId)
                .SetProperty(f => f.Notes, file.Notes)
                .SetProperty(f => f.PostalAdress, file.PostalAdress)
                .SetProperty(f => f.DeliveryMethodId, file.DeliveryMethodId)
                .SetProperty(f => f.EmailAdress, file.EmailAdress)
                .SetProperty(f => f.ReminderMethodId, file.ReminderMethodId)
                .SetProperty(f => f.RequestCountryId, file.RequestCountryId)
                .SetProperty(f => f.UserId, file.UserId)
                .SetProperty(f => f.UserNameIssuingLetter, file.UserNameIssuingLetter)
                .SetProperty(f => f.LastDelivery, file.LastDelivery)
                .SetProperty(f => f.ImporterContactingReasonId, file.ImporterContactingReasonId)
                .SetProperty(f => f.FirstProvideContactDate, file.FirstProvideContactDate)
                .SetProperty(f => f.UpdateDate, now)
                .SetProperty(f => f.UpdateUserId, userId));
        return affected > 0;
    }

    public async Task UnlinkAllRequestsFromFile(int fileId, int userId)
    {
        // SaveAuthenticationRequestFile / CheckStatusAndOpenTask CancelledFile branch: detach every child request from
        // the cancelled file (AuthenticationFileID → null) + stamp update-audit. Set-based.
        var now = DateTimeOffset.Now;
        await Context.CertificateOfOriginsImportAuthenticationRequests
            .Where(r => r.AuthenticationFileId == fileId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.AuthenticationFileId, (int?)null)
                .SetProperty(r => r.UpdateDate, now)
                .SetProperty(r => r.UpdateUserId, userId));
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
