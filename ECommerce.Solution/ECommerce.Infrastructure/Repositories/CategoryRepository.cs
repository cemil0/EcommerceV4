using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.CategorySlug == slug && c.IsActive);
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId)
    {
        return await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}
