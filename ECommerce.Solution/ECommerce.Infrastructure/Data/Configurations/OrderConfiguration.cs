using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.OrderId);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.OrderType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(o => o.OrderStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(o => o.SubtotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(o => o.CouponCode)
            .HasMaxLength(50);

        builder.Property(o => o.CustomerNotes)
            .HasMaxLength(1000);

        builder.Property(o => o.AdminNotes)
            .HasMaxLength(1000);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);

        builder.HasIndex(o => o.CompanyId);

        builder.HasIndex(o => new { o.OrderDate, o.OrderStatus });

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(o => o.Company)
            .WithMany(co => co.Orders)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(o => o.ShippingAddress)
            .WithMany()
            .HasForeignKey(o => o.ShippingAddressId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
