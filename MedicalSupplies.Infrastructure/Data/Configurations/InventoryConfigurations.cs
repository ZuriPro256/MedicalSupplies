using MedicalSupplies.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
{
    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
    {
        builder.HasKey(b => b.BatchId);
        builder.Property(b => b.BatchNumber).HasMaxLength(100);
        builder.Property(b => b.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(b => b.ExpiryDate);

        builder.HasOne(b => b.Product)
            .WithMany(p => p.Batches)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Supplier)
            .WithMany(s => s.Batches)
            .HasForeignKey(b => b.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.PurchaseOrder)
            .WithMany(po => po.Batches)
            .HasForeignKey(b => b.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.Property(m => m.CreatedBy).HasMaxLength(450);

        builder.HasOne(m => m.Product)
            .WithMany(p => p.StockMovements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Batch)
            .WithMany(b => b.StockMovements)
            .HasForeignKey(m => m.BatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Order)
            .WithMany()
            .HasForeignKey(m => m.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.OrderDetail)
            .WithMany()
            .HasForeignKey(m => m.OrderDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.PurchaseOrder)
            .WithMany()
            .HasForeignKey(m => m.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ReversesStockMovement)
            .WithMany()
            .HasForeignKey(m => m.ReversesStockMovementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
