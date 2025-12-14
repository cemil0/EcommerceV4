using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // One-to-one relationship with Customer
        builder.HasOne(u => u.Customer)
            .WithOne(c => c.ApplicationUser)
            .HasForeignKey<Customer>(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
