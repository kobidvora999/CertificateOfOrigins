using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginsDbContext : DbContext
{
    public CertificateOfOriginsDbContext(DbContextOptions<CertificateOfOriginsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateOfOrigin> CertificateOfOrigins { get; set; }

    public virtual DbSet<VerificationProhibitedImporters> VerificationProhibitedImporters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

public partial class CertificateOfOriginsDbReadOnlyContext : CertificateOfOriginsDbContext, IReadOnlyContext
{
    public CertificateOfOriginsDbReadOnlyContext(DbContextOptions<CertificateOfOriginsDbContext> options)
        : base(options)
    {
    }
}
