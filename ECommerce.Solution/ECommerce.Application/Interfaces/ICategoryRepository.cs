using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId);
}
