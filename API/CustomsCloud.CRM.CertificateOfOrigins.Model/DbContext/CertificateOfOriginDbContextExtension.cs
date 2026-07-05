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

    public async Task<IEnumerable<ExportDocumentAuthenticationRequestSearchResultDto>> GetExportDocumentAuthenticationRequestSearch(object? parameters = null, CancellationToken cancellationToken = default)
    {
        var conn = Database.GetDbConnection();
        var cmd = new CommandDefinition(
            commandText: "dbo.CROSS_ExportDocumentAuthenticationRequestSearch",
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
