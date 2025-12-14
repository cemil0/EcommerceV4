using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(pi => pi.ImageId);

        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pi => pi.AltText)
            .HasMaxLength(200);

        builder.Property(pi => pi.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pi => pi.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(pi => pi.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationship
        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(pi => pi.ProductId);
        builder.HasIndex(pi => new { pi.ProductId, pi.IsPrimary });
    }
}
