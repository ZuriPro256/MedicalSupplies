using MedicalSupplies.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasCheckConstraint(
            "CK_SupplierPayments_Amount_Positive",
            "\"Amount\" > 0");

        builder.HasCheckConstraint(
            "CK_SupplierPayments_Reversal_State",
            """
            (
                "IsReversed" = FALSE
                AND "ReversedDate" IS NULL
                AND "ReversedBy" IS NULL
                AND "ReversalReason" IS NULL
            )
            OR
            (
                "IsReversed" = TRUE
                AND "ReversedDate" IS NOT NULL
                AND "ReversedBy" IS NOT NULL
                AND btrim("ReversedBy") <> ''
                AND "ReversalReason" IS NOT NULL
                AND btrim("ReversalReason") <> ''
            )
            """);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.Property(p => p.RecordedBy)
            .HasMaxLength(450);

        builder.Property(p => p.ReversedBy)
            .HasMaxLength(450);

        builder.Property(p => p.ReversalReason)
            .HasMaxLength(1000);

        builder.HasIndex(p => p.PurchaseOrderId);

        builder.HasOne(p => p.PurchaseOrder)
            .WithMany(po => po.SupplierPayments)
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
