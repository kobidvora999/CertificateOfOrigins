using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.DAL;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext
{
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

    public async Task<CertificateOfOriginDto?> GetCertificateOfOriginById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_GetCertificateOfOriginByID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);

        using var grid = await conn.QueryMultipleAsync(cmd);

        var certificateOfOrigin = (await grid.ReadAsync<CertificateOfOriginDto>()).FirstOrDefault();
        if (certificateOfOrigin == null)
        {
            return null;
        }

        certificateOfOrigin.StakeholdersIds = new List<int> { certificateOfOrigin.CustomerId, certificateOfOrigin.CreateCustomerId };

        var vsDeclarationErrors = (await grid.ReadAsync<CertificateOfOriginVsDeclarationErrorDto>()).ToList();
        var detailsTypeCodes = (await grid.ReadAsync<CertificateDetailsTypeCodeEnumDto>()).ToList();
        var details = (await grid.ReadAsync<CertificateOfOriginDetailsDto>()).ToList();
        var invoices = (await grid.ReadAsync<CertificateOfOriginInvoiceDetailDto>()).ToList();
        var itemDetails = (await grid.ReadAsync<CertificateOfOriginItemDetailDto>()).ToList();
        var milestones = (await grid.ReadAsync<CertificateMilestonesDto>()).ToList();

        certificateOfOrigin.Milestones.AddRange(milestones);
        certificateOfOrigin.CertificateOfOriginVsDeclarationError.AddRange(vsDeclarationErrors);

        foreach (var detail in details)
        {
            detail.CertificateDetailsTypeCode = detailsTypeCodes.FirstOrDefault(d => d.Id == detail.CertificateDetailsTypeCodeId);
        }
        certificateOfOrigin.CertificateOfOriginDetails.AddRange(details);

        foreach (var invoice in invoices)
        {
            var thisItemDetails = itemDetails.Where(i => i.CertificateOfOriginInvoiceDetailId == invoice.Id).ToList();
            if (thisItemDetails.Count > 0)
            {
                invoice.CertificateOfOriginItemDetail.AddRange(thisItemDetails);
            }
            certificateOfOrigin.CertificateOfOriginInvoiceDetail.Add(invoice);
        }

        return certificateOfOrigin;
    }
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }
}
