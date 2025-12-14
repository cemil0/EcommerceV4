using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(ECommerceDbContext context) : base(context) { }

    public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId)
    {
        return await _dbSet
            .Where(pi => pi.ProductId == productId)
            .OrderByDescending(pi => pi.IsPrimary)
            .ThenBy(pi => pi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ProductImage?> GetPrimaryImageAsync(int productId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.IsPrimary);
    }

    public async Task SetPrimaryImageAsync(int productId, int imageId)
    {
        // Remove primary flag from all images of this product
        var images = await _dbSet.Where(pi => pi.ProductId == productId).ToListAsync();
        foreach (var img in images)
        {
            img.IsPrimary = img.ImageId == imageId;
        }
        await _context.SaveChangesAsync();
    }
}
