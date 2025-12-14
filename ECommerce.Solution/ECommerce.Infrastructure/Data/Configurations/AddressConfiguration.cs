using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.AddressId);

        builder.Property(a => a.AddressType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.AddressTitle)
            .HasMaxLength(100);

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(500);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.District)
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.CustomerId);

        // Relationships
        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
