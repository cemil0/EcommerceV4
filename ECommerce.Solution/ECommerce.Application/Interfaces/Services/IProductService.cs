using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto?> GetBySlugAsync(string slug);
    
    // Legacy methods (kept for backward compatibility)
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 10);
    Task<IEnumerable<ProductDto>> SearchAsync(string searchTerm);
    
    // NEW: Paginated methods
    Task<PagedResponse<ProductDto>> GetPagedAsync(PagedRequest request);
    Task<PagedResponse<ProductDto>> GetPagedByCategoryAsync(int categoryId, PagedRequest request);
    Task<PagedResponse<ProductDto>> SearchPagedAsync(string searchTerm, PagedRequest request);
}
