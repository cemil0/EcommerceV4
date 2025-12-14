using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.CustomerId);

        builder.Property(c => c.ApplicationUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(c => c.CustomerType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Gender)
            .HasMaxLength(10);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasIndex(c => c.ApplicationUserId);

        // Relationships
        builder.HasOne(c => c.Company)
            .WithMany(co => co.Customers)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Carts)
            .WithOne(ca => ca.Customer)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
