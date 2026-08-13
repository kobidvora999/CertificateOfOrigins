using CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CustomsCloud.InfrastructureCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CustomsCloud.CRM.CertificateOfOrigins.DAL;

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

    public virtual DbSet<VerificationProhibitedImporters> VerificationProhibitedImporters { get; set; }

    public virtual DbSet<ExportDocumentAuthenticationRequest> ExportDocumentAuthenticationRequests { get; set; }

    public virtual DbSet<CertificateOfOriginsImportAuthenticationRequest> CertificateOfOriginsImportAuthenticationRequests { get; set; }

    public virtual DbSet<CertificateOfOriginsImportAuthenticationFileDetails> CertificateOfOriginsImportAuthenticationFileDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsItemDetails> CertificateOfOriginsItemDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsDecision> CertificateOfOriginsDecisions { get; set; }

    public virtual DbSet<CertificateOfOriginsSupplierDeliveryCountryConfig> CertificateOfOriginsSupplierDeliveryCountryConfigs { get; set; }

    public virtual DbSet<CertificateOfOriginsAuthenticationFileStatus> CertificateOfOriginsAuthenticationFileStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The legacy SQL columns are `datetime` (CLR DateTime) while the entities map them as DateTimeOffset (repo
        // convention). Without this converter an EF LINQ read that materializes such a column throws
        // InvalidCastException (DateTime -> DateTimeOffset). Applied per-property here (not via ConfigureConventions)
        // so no EF type is exposed on the Model project's public API (avoids an EF-version reference conflict).
        var converter = new ValueConverter<DateTimeOffset, DateTime>(
            offset => offset.DateTime,
            dateTime => new DateTimeOffset(dateTime, TimeSpan.Zero));
        var nullableConverter = new ValueConverter<DateTimeOffset?, DateTime?>(
            offset => offset.HasValue ? offset.Value.DateTime : null,
            dateTime => dateTime.HasValue ? new DateTimeOffset(dateTime.Value, TimeSpan.Zero) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(converter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(nullableConverter);
                }
            }
        }

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
