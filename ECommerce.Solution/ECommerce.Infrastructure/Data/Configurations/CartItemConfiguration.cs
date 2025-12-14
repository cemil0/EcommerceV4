using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.CartItemId);

        builder.Property(ci => ci.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ci => ci.TotalPrice)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

        builder.HasIndex(ci => ci.CartId);

        builder.HasIndex(ci => ci.ProductVariantId);

        // Relationships
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany(pv => pv.CartItems)
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
