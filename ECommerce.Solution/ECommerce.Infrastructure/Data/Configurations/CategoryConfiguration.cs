using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CategorySlug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.MetaTitle)
            .HasMaxLength(200);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(500);

        builder.Property(c => c.MetaKeywords)
            .HasMaxLength(500);

        builder.HasIndex(c => c.CategorySlug)
            .IsUnique();

        builder.HasIndex(c => c.ParentCategoryId);

        // Self-referencing relationship
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
