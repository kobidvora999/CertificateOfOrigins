using Microsoft.EntityFrameworkCore;
using CustomsCloud.InfrastructureCore.DAL;
using CustomsCloud.InfrastructureCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext
{
}

public partial class CertificateOfOriginDbReadOnlyContext : CertificateOfOriginDbContext, IReadOnlyContext
{
    public CertificateOfOriginDbReadOnlyContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
        ReadOnlyDbContext.SetDbContextReadOnlyConnectionString(this);
    }
}
