using MedicalSupplies.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class PurchaseOrderReturnConfiguration : IEntityTypeConfiguration<PurchaseOrderReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderReturn> builder)
    {
        builder.HasKey(r => r.PurchaseOrderReturnId);

        builder.Property(r => r.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.RecordedBy)
            .HasMaxLength(450);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasIndex(r => r.ReturnNumber)
            .IsUnique();

        builder.HasIndex(r => r.PurchaseOrderId);

        builder.HasOne(r => r.PurchaseOrder)
            .WithMany(po => po.Returns)
            .HasForeignKey(r => r.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderReturnDetailConfiguration
    : IEntityTypeConfiguration<PurchaseOrderReturnDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderReturnDetail> builder)
    {
        builder.HasKey(r => r.PurchaseOrderReturnDetailId);

        builder.Property(r => r.QuantityReturned)
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PurchaseOrderReturnDetail_QuantityReturned",
            "\"QuantityReturned\" > 0"));

        builder.Property(r => r.UnitCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasOne(r => r.PurchaseOrderReturn)
            .WithMany(r => r.Details)
            .HasForeignKey(r => r.PurchaseOrderReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.PurchaseOrderDetail)
            .WithMany()
            .HasForeignKey(r => r.PurchaseOrderDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Batch)
            .WithMany()
            .HasForeignKey(r => r.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.BatchId);
    }
}
