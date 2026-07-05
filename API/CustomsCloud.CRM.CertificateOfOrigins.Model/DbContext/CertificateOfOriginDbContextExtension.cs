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
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }
}
