using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class ECommerceDbContext : IdentityDbContext<ApplicationUser>
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    public DbSet<BulkImportJob> BulkImportJobs { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ECommerceDbContext).Assembly);
    }
}
