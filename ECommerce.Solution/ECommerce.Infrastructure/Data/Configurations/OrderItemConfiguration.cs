using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.OrderItemId);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(oi => oi.VariantName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(oi => oi.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(oi => oi.TotalPrice)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("(([Quantity] * [UnitPrice]) - [DiscountAmount]) + [TaxAmount]", stored: true);

        builder.HasIndex(oi => oi.OrderId);

        builder.HasIndex(oi => oi.ProductVariantId);

        // Relationships
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany(pv => pv.OrderItems)
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
