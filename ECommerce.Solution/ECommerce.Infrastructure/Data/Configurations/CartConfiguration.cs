using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.CartId);

        builder.Property(c => c.SessionId)
            .HasMaxLength(200);

        // Ignore computed properties
        builder.Ignore(c => c.TotalAmount);
        builder.Ignore(c => c.TotalItems);

        builder.HasIndex(c => c.CustomerId);

        builder.HasIndex(c => c.SessionId);

        // Relationships
        builder.HasOne(c => c.Customer)
            .WithMany(cu => cu.Carts)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
