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
