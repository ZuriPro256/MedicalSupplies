using MedicalSupplies.Core.Entities;
using MedicalSupplies.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalSupplies.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.Property(c => c.CategoryName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.Property(b => b.BrandName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Country).HasMaxLength(100);
        builder.Property(b => b.Description).HasMaxLength(500);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.ProductCode).IsUnique();

        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(250);
        builder.HasIndex(p => p.ProductName);

        builder.Property(p => p.UnitOfMeasure).HasMaxLength(50);
        builder.Property(p => p.PackSize).HasMaxLength(100);
        builder.Property(p => p.CountryOfOrigin).HasMaxLength(100);

        builder.Property(p => p.SellingPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Availability)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(ProductAvailability.InStock);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.HasKey(h => h.ProductPriceHistoryId);

        builder.Property(h => h.PriceType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(h => h.PreviousAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(h => h.NewAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(h => h.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("UGX");

        builder.Property(h => h.ChangedBy)
            .HasMaxLength(200);

        builder.Property(h => h.Reason)
            .HasMaxLength(500);

        builder.Property(h => h.ChangedDate)
            .IsRequired();

        builder.HasIndex(h => h.ProductId);
        builder.HasIndex(h => h.ChangedDate);

        builder.HasOne(h => h.Product)
            .WithMany()
            .HasForeignKey(h => h.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
