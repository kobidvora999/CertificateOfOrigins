using System;
using System.Collections.Generic;
using IssueCertificateOfOrigin.Worker.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueCertificateOfOrigin.Worker;

public partial class CustomsDbContext : DbContext
{
    public CustomsDbContext(DbContextOptions<CustomsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CertificateOfOriginsCertificateOfOrigin> CertificateOfOriginsCertificateOfOrigins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificateOfOriginsCertificateOfOrigin>(entity =>
        {
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.QrcodePath).HasDefaultValueSql("((1))");
            entity.Property(e => e.State).HasDefaultValue(1);
            entity.Property(e => e.TimeStamp)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CertificateIdtoCancelNavigation).WithMany(p => p.InverseCertificateIdtoCancelNavigation).HasConstraintName("FK_CertificateOfOrigins_CertificateOfOrigin_CertificateOfOrigins_CertificateOfOrigin");
        });
        modelBuilder.HasSequence<int>("Richbit990", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit991", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit992", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit993", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit994", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit995", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit996", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit997", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit998", "StockPileData")
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("Richbit999", "StockPileData")
            .StartsAt(113065L)
            .HasMin(0L)
            .HasMax(9999999L);
        modelBuilder.HasSequence<int>("sq_CargoControl_Cargo_ID", "Logistics");
        modelBuilder.HasSequence<int>("sq_CargoControl_CargoRelatedAddressContactPhone_ID", "Logistics");
        modelBuilder.HasSequence<int>("sq_CargoControl_CargoRowDetail_ID", "Logistics");
        modelBuilder.HasSequence<int>("sq_CargoControl_CargoVersion_ID", "Logistics");
        modelBuilder.HasSequence("sq_CargoControl_CargoVersionPostalDetail_ID", "Logistics").StartsAt(30763L);
        modelBuilder.HasSequence<int>("sq_CargoControl_Manifest_ID", "Logistics");
        modelBuilder.HasSequence<int>("sq_CargoControl_TransportationMean_ID", "Logistics");
        modelBuilder.HasSequence("sq_CertificateOfOrigins_CertificateOfOrigin", "CRM").StartsAt(70000L);
        modelBuilder.HasSequence<int>("sq_Collateral_CollateralRequest", "FinanceInfr");
        modelBuilder.HasSequence("sq_CustomsItemLinkageComputedData", "Temp")
            .HasMin(1L)
            .HasMax(2147483647L);
        modelBuilder.HasSequence<int>("sq_DealFile_AvailabilityLog_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_DealFile_Consignment_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_DealFile_GoodsItem_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_DealFile_Invoice_ID", "CRP");
        modelBuilder.HasSequence("sq_DealFileValidation_ExceptionConstraint_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_Evaluation_ProceduralFaultDetail_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_ExportDealFile_ExportConsignment_ID", "CRP");
        modelBuilder.HasSequence<int>("sq_ExportDealFile_ExportGoodsItem_ID", "CRP").StartsAt(0L);
        modelBuilder.HasSequence<int>("sq_ExportDealFile_ExportInvoice_ID", "CRP").StartsAt(0L);
        modelBuilder.HasSequence<int>("sq_ExportDealFile_TransshipmentImportConsignment_ID", "CRP").HasMin(1L);
        modelBuilder.HasSequence<int>("sq_ExportDealFile_TransshipmentImportIntermediateSite_ID", "CRP").HasMin(1L);
        modelBuilder.HasSequence("sq_ExportLeadDocumentStatusLogisticEvents", "_Admin_").HasMin(0L);
        modelBuilder.HasSequence<int>("sq_GatepassRequestDetail_GatepassNumber", "Logistics")
            .StartsAt(1000000000L)
            .HasMin(0L);
        modelBuilder.HasSequence<int>("sq_General_ID", "_Admin_");
        modelBuilder.HasSequence<int>("sq_Guarantee_GuaranteeDigitalCode", "FinanceInfr").HasMin(0L);
        modelBuilder.HasSequence<int>("sq_Guarantee_GuaranteeRequest_Title", "FinanceInfr").HasMin(0L);
        modelBuilder.HasSequence<int>("sq_LeadDocumentStatusLogisticEvents", "_Admin_").HasMin(0L);
        modelBuilder.HasSequence<int>("sq_LeadDocumentStatusSerialQueue", "_Admin_");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
