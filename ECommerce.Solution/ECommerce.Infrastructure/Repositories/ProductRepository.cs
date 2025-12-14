using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .FirstOrDefaultAsync(p => p.ProductSlug == slug && p.IsActive);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.ProductVariants)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.ProductVariants)
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> ExistsBySKUAsync(string sku)
    {
        return await _dbSet.AnyAsync(p => p.SKU == sku);
    }

    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }
}
