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

    public virtual DbSet<ImportAuthenticationRequest> ImportAuthenticationRequests { get; set; }

    public virtual DbSet<PrefernceDocumentType> PrefernceDocumentTypes { get; set; }

    public virtual DbSet<ImportAuthenticationFileDetails> ImportAuthenticationFileDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsItemDetails> CertificateOfOriginsItemDetails { get; set; }

    public virtual DbSet<CertificateOfOriginsDecision> CertificateOfOriginsDecisions { get; set; }

    public virtual DbSet<CertificateOfOriginsAuthenticationFileStatus> CertificateOfOriginsAuthenticationFileStatuses { get; set; }

    public virtual DbSet<CertificateOfOriginSupplierDeliveryCountryConfig> CertificateOfOriginSupplierDeliveryCountryConfigs { get; set; }

    public virtual DbSet<ExportDocumentAuthenticationRequest> ExportDocumentAuthenticationRequests { get; set; }

    public virtual DbSet<ExportDocumentAuthenticationRequestLeadDocument> ExportDocumentAuthenticationRequestLeadDocuments { get; set; }

    public virtual DbSet<ExportAuthenticationRequestStatus> ExportAuthenticationRequestStatuses { get; set; }

    public virtual DbSet<CustomsItemToExportDocumentAuthenticationRequest> CustomsItemToExportDocumentAuthenticationRequests { get; set; }

    public virtual DbSet<ExportAuthenticationRequestManufacturingArea> ExportAuthenticationRequestManufacturingAreas { get; set; }

    public virtual DbSet<VerificationProhibitedImporters> VerificationProhibitedImporters { get; set; }

    public virtual DbSet<CertificateOfOriginTypeCode> CertificateOfOriginTypeCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
