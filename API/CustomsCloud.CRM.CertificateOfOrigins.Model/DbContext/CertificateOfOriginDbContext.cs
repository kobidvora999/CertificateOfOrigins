using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext : DbContext
{
    public CertificateOfOriginDbContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateOfOriginsDecision> CertificateOfOriginsDecisions { get; set; }

    public virtual DbSet<CertificateOfOriginSupplierDeliveryCountryConfig> CertificateOfOriginSupplierDeliveryCountryConfigs { get; set; }

    public virtual DbSet<VerificationProhibitedImporters> VerificationProhibitedImporters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
