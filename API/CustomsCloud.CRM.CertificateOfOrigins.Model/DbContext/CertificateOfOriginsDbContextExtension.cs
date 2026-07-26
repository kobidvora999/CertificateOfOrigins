using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginsDbContext
{
    // dbo.GetCertificateOfOriginsByFilter — dynamic-SQL search; exporter/agent titles are NULL from the SP
    // (customer JOINs removed) and enriched in the BL via the Customers proxy. A search legitimately returns
    // an empty set, so no row-count assertion is applied.
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

    // dbo.CheckIfExistsAdditionalRequestsForVendor — scalar bit: >1 import-authentication request for the
    // vendor within the last 3 years.
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

    // dbo.CheckIfExistsAdditionalRequestsForImporter — scalar bit: an additional import-authentication request
    // exists for the importer within the last @DaysForLastDelivery days (config, read inside the SP from the
    // local Infrastructure.Parameters), branching on vendor vs customer by the country's delivery config.
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
}
