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

    public async Task<IEnumerable<GetImportAuthenticationRequestResultDto>> GetImportAuthenticationRequestByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_GetImportAuthenticationRequestByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<GetImportAuthenticationRequestResultDto>(cmd);
        DapperHelper.DapperCheckRows(cmd, result);
        return result;
    }

    public async Task<ImportAuthenticationRequestDto?> GetAuthenticationRequestById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_CertificateOfOrigins_GetImportAuthenticationRequestById",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);

        using var grid = await conn.QueryMultipleAsync(cmd);

        var request = (await grid.ReadAsync<ImportAuthenticationRequestDto>()).FirstOrDefault();
        if (request == null)
        {
            return null;
        }

        var itemDetails = (await grid.ReadAsync<CertificateOfOriginsItemDetailDto>()).ToList();
        request.ItemDetails.AddRange(itemDetails);

        var document = (await grid.ReadAsync<DocumentDto>()).FirstOrDefault();
        request.Document = document;

        return request;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForVendor",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
        return result;
    }

    public async Task<ImportAuthenticationFileDetailsDto?> GetAuthenticationFileDetailsById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_GetImportAuthenticationFileDetailsAndRequests",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);

        using var grid = await conn.QueryMultipleAsync(cmd);

        var file = (await grid.ReadAsync<ImportAuthenticationFileDetailsDto>()).FirstOrDefault();
        if (file == null)
        {
            return null;
        }

        var requests = (await grid.ReadAsync<ImportAuthenticationRequestDto>()).ToList();
        var documents = (await grid.ReadAsync<DocumentDto>()).ToList();
        var itemDetails = (await grid.ReadAsync<CertificateOfOriginsItemDetailDto>()).ToList();

        foreach (var request in requests)
        {
            request.Document = documents.FirstOrDefault(d => d.Id == request.DocumentId);
            request.ItemDetails.AddRange(itemDetails.Where(i => i.ImportAuthenticationRequestId == request.DocumentId));
            request.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
            {
                { 1055 /* EEntityType.ImportDeclaration */, new List<int> { request.LeadDocumentId } }
            };
        }

        file.Requests.AddRange(requests);
        file.EntityTypeAndIdsToSearch = new Dictionary<int, List<int>>
        {
            { 1055 /* EEntityType.ImportDeclaration */, requests.Select(r => r.LeadDocumentId).ToList() }
        };

        if (file.Requests.Count > 0)
        {
            file.OrganizationUnitId = file.Requests[0].OrganizationUnitId;
        }

        if (file.CustomerId == 0)
        {
            file.CustomerId = -1;
        }

        return file;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "CRM.usp_CertificateOfOrigins_CheckIfExistsAdditionalRequestsForImporter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
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
