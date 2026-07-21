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
}
