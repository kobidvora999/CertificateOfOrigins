using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;
using Microsoft.EntityFrameworkCore;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

public partial class CertificateOfOriginDbContext : DbContext
{
    public CertificateOfOriginDbContext(DbContextOptions<CertificateOfOriginDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateOfOrigin> CertificateOfOrigins { get; set; }

    public virtual DbSet<CertificateOfOriginStatusCode> CertificateOfOriginStatusCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
