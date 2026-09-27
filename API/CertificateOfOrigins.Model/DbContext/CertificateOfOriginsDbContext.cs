using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;

namespace CertificateOfOrigins.DAL;

public partial class CertificateOfOriginsDbContext : DbContext
{
    public CertificateOfOriginsDbContext(DbContextOptions<CertificateOfOriginsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateOfOrigin> CertificateOfOrigins { get; set; }

    public virtual DbSet<CertificateOfOriginDetails> CertificateOfOriginDetails { get; set; }

    public virtual DbSet<CertificateOfOriginVsDeclarationError> CertificateOfOriginVsDeclarationErrors { get; set; }

    public virtual DbSet<CertificateOfOriginInvoiceDetail> CertificateOfOriginInvoiceDetails { get; set; }

    public virtual DbSet<CertificateOfOriginItemDetail> CertificateOfOriginItemDetails { get; set; }

    public virtual DbSet<CertificateOfOriginTypeCode> CertificateOfOriginTypeCodes { get; set; }

    public virtual DbSet<DetailsPerCertificate> DetailsPerCertificates { get; set; }

    public virtual DbSet<OriginCriterion> OriginCriterions { get; set; }

    public virtual DbSet<VerificationProhibitedImporters> VerificationProhibitedImporters { get; set; }

    public virtual DbSet<ExportDocumentAuthenticationRequest> ExportDocumentAuthenticationRequests { get; set; }

    public virtual DbSet<PreferenceDocumentType> PreferenceDocumentTypes { get; set; }

    public virtual DbSet<ExportAuthenticationRequestStatus> ExportAuthenticationRequestStatuses { get; set; }

    public virtual DbSet<CertificateOfOriginsImportAuthenticationRequest> CertificateOfOriginsImportAuthenticationRequests { get; set; }

    public virtual DbSet<CertificateOfOriginsImportAuthenticationFileDetails> CertificateOfOriginsImportAuthenticationFileDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsItemDetails> CertificateOfOriginsItemDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsDecision> CertificateOfOriginsDecisions { get; set; }

    public virtual DbSet<CertificateOfOriginsSupplierDeliveryCountryConfig> CertificateOfOriginsSupplierDeliveryCountryConfigs { get; set; }

    public virtual DbSet<CertificateOfOriginsAuthenticationFileStatus> CertificateOfOriginsAuthenticationFileStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Date columns are SQL datetime/date and map natively to DateTime — no value converter.
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
