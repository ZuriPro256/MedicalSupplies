using MedicalSupplies.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.Property(s => s.SupplierCode).IsRequired().HasMaxLength(30);
        builder.HasIndex(s => s.SupplierCode).IsUnique();
        builder.Property(s => s.SupplierName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.ContactPerson).HasMaxLength(150);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.TaxNumber).HasMaxLength(50);
        builder.Property(s => s.Notes).HasMaxLength(1000);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.Property(po => po.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(po => po.PurchaseOrderNumber).IsUnique();
        builder.Property(po => po.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(po => po.DeliveryCost).HasColumnType("decimal(18,2)");
        builder.Property(po => po.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(po => po.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(po => po.Currency).IsRequired().HasMaxLength(10);
        builder.Property(po => po.Notes).HasMaxLength(1000);
        builder.Property(po => po.CreatedBy).HasMaxLength(450);
        builder.Property(po => po.ApprovedBy).HasMaxLength(450);
        builder.Property(po => po.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(po => po.PaymentStatus).HasConversion<string>().HasMaxLength(30);

        builder.HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.Property(d => d.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(d => d.TotalCost).HasColumnType("decimal(18,2)");
        builder.Ignore(d => d.QuantityRemaining);

        builder.HasOne(d => d.PurchaseOrder)
            .WithMany(po => po.Details)
            .HasForeignKey(d => d.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
