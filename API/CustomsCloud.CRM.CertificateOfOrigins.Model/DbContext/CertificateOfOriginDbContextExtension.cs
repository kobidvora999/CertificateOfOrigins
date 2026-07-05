using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.DAL;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext
{
    public async Task<int> UpdateImportAuthenticationRequest(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.UpdateImportAuthenticationRequest",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteAsync(cmd);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForVendor(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.CheckIfExistsAdditionalRequestsForVendor",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
        return result;
    }

    public async Task<bool> CheckIfExistsAdditionalRequestsForImporter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.CheckIfExistsAdditionalRequestsForImporter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.ExecuteScalarAsync<bool>(cmd);
        return result;
    }

    public async Task<IEnumerable<ImportAuthenticationRequestByLeadDocumentDto>> GetAuthenticationRequestByLeadDocumentID(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetAuthenticationRequestByLeadDocumentID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<ImportAuthenticationRequestByLeadDocumentDto>(cmd);
        return result;
    }

    public async Task<(ImportAuthenticationFileDetailsDto? File, List<ImportAuthenticationRequestDto> Requests, List<CertificateOfOriginsItemDetailDto> ItemDetails)> GetImportAuthenticationFileDetailsAndRequests(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetImportAuthenticationFileDetailsAndRequests",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        using var multi = await conn.QueryMultipleAsync(cmd);
        var file = (await multi.ReadAsync<ImportAuthenticationFileDetailsDto>()).FirstOrDefault();
        var requests = (await multi.ReadAsync<ImportAuthenticationRequestDto>()).ToList();
        _ = await multi.ReadAsync();

        // 3rd result set (documents) is intentionally empty — documents come from the Documents microservice
        var itemDetails = (await multi.ReadAsync<CertificateOfOriginsItemDetailDto>()).ToList();
        return (file, requests, itemDetails);
    }

    public async Task<(CertificateOfOriginDto? Certificate, List<CertificateMilestoneRowDto> MilestoneRows)> GetCertificateOfOriginByID(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetCertificateOfOriginByID",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        using var multi = await conn.QueryMultipleAsync(cmd);
        var certificate = (await multi.ReadAsync<CertificateOfOriginDto>()).FirstOrDefault();
        var errors = (await multi.ReadAsync<CertificateOfOriginVsDeclarationErrorDto>()).ToList();
        var detailsTypeCodes = (await multi.ReadAsync<CertificateDetailsTypeCodeEnumDto>()).ToList();
        var details = (await multi.ReadAsync<CertificateOfOriginDetailsDto>()).ToList();
        var invoices = (await multi.ReadAsync<CertificateOfOriginInvoiceDetailDto>()).ToList();
        var items = (await multi.ReadAsync<CertificateOfOriginItemDetailDto>()).ToList();
        var milestoneRows = (await multi.ReadAsync<CertificateMilestoneRowDto>()).ToList();
        if (certificate == null)
        {
            return (null, milestoneRows);
        }

        foreach (var detail in details)
        {
            detail.CertificateDetailsTypeCode = detailsTypeCodes.FirstOrDefault(t => t.Id == detail.CertificateDetailsTypeCodeId);
        }

        foreach (var invoice in invoices)
        {
            invoice.CertificateOfOriginItemDetail = items.Where(d => d.CertificateOfOriginInvoiceDetailId == invoice.Id).ToList();
        }

        certificate.CertificateOfOriginVsDeclarationError = errors;
        certificate.CertificateOfOriginDetails = details;
        certificate.CertificateOfOriginInvoiceDetail = invoices;
        return (certificate, milestoneRows);
    }

    public async Task<IEnumerable<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.ExportDocumentAuthenticationRequestSearch",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<ExportDocumentAuthenticationRequestSearchResultDto>(cmd);
        return result;
    }

    public async Task<IEnumerable<GetImportAuthenticationRequestResultDto>> GetAuthenticationRequestByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetImportAuthenticationRequestByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<GetImportAuthenticationRequestResultDto>(cmd);
        return result;
    }

    public async Task<IEnumerable<CertificateOfOriginResultDto>> GetCertificateOfOriginsByFilter(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetCertificateOfOriginsByFilter",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        var result = await conn.QueryAsync<CertificateOfOriginResultDto>(cmd);
        return result;
    }

    public async Task<(ImportAuthenticationRequestDto? Request, List<CertificateOfOriginsItemDetailDto> ItemDetails)> GetImportAuthenticationRequestById(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.GetImportAuthenticationRequestById",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken,
            parameters: parameters);
        using var multi = await conn.QueryMultipleAsync(cmd);
        var request = (await multi.ReadAsync<ImportAuthenticationRequestDto>()).FirstOrDefault();
        var itemDetails = (await multi.ReadAsync<CertificateOfOriginsItemDetailDto>()).ToList();

        // 3rd result set (documents) is intentionally empty — documents come from the Documents microservice
        return (request, itemDetails);
    }
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }
}
