using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.CompanyId);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TaxNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.TaxOffice)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CompanyType)
            .HasMaxLength(50);

        builder.Property(c => c.Industry)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        builder.HasIndex(c => c.TaxNumber)
            .IsUnique();

        // Relationships
        builder.HasMany(c => c.Customers)
            .WithOne(cu => cu.Company)
            .HasForeignKey(cu => cu.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Company)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
