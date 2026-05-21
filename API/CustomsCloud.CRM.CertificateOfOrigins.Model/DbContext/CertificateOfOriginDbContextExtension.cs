using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.DAL;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext
{
    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_GetCertificateOfOriginByID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        using var multi = await conn.QueryMultipleAsync(cmd);
        var certificate = (await multi.ReadAsync<CertificateOfOriginDto>()).FirstOrDefault();
        if (certificate == null) { return null; }
        var declarationErrors = (await multi.ReadAsync<CertificateOfOriginVsDeclarationErrorDto>()).ToList();
        var detailsTypeCodes = (await multi.ReadAsync<CertificateDetailsTypeCodeEnumDto>()).ToList();
        var details = (await multi.ReadAsync<CertificateOfOriginDetailsDto>()).ToList();
        var invoices = (await multi.ReadAsync<CertificateOfOriginInvoiceDetailDto>()).ToList();
        var itemDetails = (await multi.ReadAsync<CertificateOfOriginItemDetailDto>()).ToList();
        var milestones = (await multi.ReadAsync<CertificateMilestoneDto>()).ToList();
        certificate.StakeholderIds = [certificate.CustomerId, certificate.CreateCustomerId];
        certificate.DeclarationErrors = declarationErrors;
        foreach (var detail in details)
        {
            detail.CertificateDetailsTypeCode = detailsTypeCodes.FirstOrDefault(d => d.Id == detail.CertificateDetailsTypeCodeId);
        }

        certificate.Details = details;
        foreach (var invoice in invoices)
        {
            invoice.Items = itemDetails.Where(i => i.CertificateOfOriginInvoiceDetailId == invoice.Id).ToList();
        }

        certificate.Invoices = invoices;
        certificate.Milestones = milestones;
        return certificate;
    }

    public async Task<IEnumerable<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<CertificateOfOriginResultDto>(cmd);
        DapperHelper.DapperCheckRows(cmd, result);
        return result;
    }
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }
}
