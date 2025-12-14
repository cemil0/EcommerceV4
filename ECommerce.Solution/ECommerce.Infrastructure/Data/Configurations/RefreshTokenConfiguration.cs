using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.RefreshTokenId);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(45); // IPv4/IPv6

        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(100);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        // Filtered index for active tokens only
        builder.HasIndex(rt => rt.UserId)
            .HasFilter("[IsRevoked] = 0 AND [IsUsed] = 0")
            .HasDatabaseName("IX_RefreshTokens_UserId_IsActive");

        // Relationship
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
