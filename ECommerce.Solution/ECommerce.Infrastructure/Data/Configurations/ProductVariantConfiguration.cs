using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(pv => pv.ProductVariantId);

        builder.Property(pv => pv.VariantSKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pv => pv.VariantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.Color)
            .HasMaxLength(50);

        builder.Property(pv => pv.Size)
            .HasMaxLength(50);

        builder.Property(pv => pv.Storage)
            .HasMaxLength(50);

        builder.Property(pv => pv.RAM)
            .HasMaxLength(50);

        builder.Property(pv => pv.BasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pv => pv.SalePrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(pv => pv.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(pv => pv.Weight)
            .HasPrecision(10, 2);

        builder.Property(pv => pv.Length)
            .HasPrecision(10, 2);

        builder.Property(pv => pv.Width)
            .HasPrecision(10, 2);

        builder.Property(pv => pv.Height)
            .HasPrecision(10, 2);

        builder.Property(pv => pv.Barcode)
            .HasMaxLength(50);

        builder.Property(pv => pv.EAN)
            .HasMaxLength(50);

        builder.HasIndex(pv => pv.VariantSKU)
            .IsUnique();

        builder.HasIndex(pv => pv.ProductId);

        // Relationships
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pv => pv.CartItems)
            .WithOne(ci => ci.ProductVariant)
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(pv => pv.OrderItems)
            .WithOne(oi => oi.ProductVariant)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
