using MedicalSupplies.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class AccountRecoveryRequestConfiguration
    : IEntityTypeConfiguration<AccountRecoveryRequest>
{
    public void Configure(EntityTypeBuilder<AccountRecoveryRequest> builder)
    {
        builder.HasKey(r => r.AccountRecoveryRequestId);

        builder.Property(r => r.RecoveryType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.SubmittedEmail)
            .HasMaxLength(150);

        builder.Property(r => r.SubmittedPhone)
            .HasMaxLength(50);

        builder.Property(r => r.NewContactPhone)
            .HasMaxLength(50);

        builder.Property(r => r.Reason)
            .HasMaxLength(1000);

        builder.Property(r => r.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.VerificationMethod)
            .HasMaxLength(100);

        builder.Property(r => r.VerificationNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.ReviewedBy)
            .HasMaxLength(450);

        builder.HasIndex(r => r.CustomerId);

        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
