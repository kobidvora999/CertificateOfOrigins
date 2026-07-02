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
}
