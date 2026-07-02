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

    public virtual DbSet<CertificateOfOriginVsDeclarationError> CertificateOfOriginVsDeclarationErrors { get; set; }

    public virtual DbSet<CertificateDetailsTypeCode> CertificateDetailsTypeCodes { get; set; }

    public virtual DbSet<CertificateOfOriginDetails> CertificateOfOriginDetails { get; set; }

    public virtual DbSet<CertificateOfOriginInvoiceDetail> CertificateOfOriginInvoiceDetails { get; set; }

    public virtual DbSet<CertificateOfOriginItemDetail> CertificateOfOriginItemDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
