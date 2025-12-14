using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.ProductSlug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(500);

        builder.Property(p => p.LongDescription)
            .HasMaxLength(4000);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.Property(p => p.Manufacturer)
            .HasMaxLength(100);

        builder.Property(p => p.Model)
            .HasMaxLength(100);

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.MetaKeywords)
            .HasMaxLength(500);

        builder.HasIndex(p => p.SKU)
            .IsUnique();

        builder.HasIndex(p => p.ProductSlug)
            .IsUnique();

        builder.HasIndex(p => p.CategoryId);

        builder.HasIndex(p => new { p.IsActive, p.IsFeatured });

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ProductVariants)
            .WithOne(pv => pv.Product)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
