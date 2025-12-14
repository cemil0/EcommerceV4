using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto?> GetBySlugAsync(string slug);
    Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();
    Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentCategoryId);
}
