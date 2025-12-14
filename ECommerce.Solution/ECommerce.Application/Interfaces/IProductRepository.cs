using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count);
    Task<bool> ExistsBySKUAsync(string sku);
    Task<IEnumerable<Product>> GetAllWithDetailsAsync();
    Task<Product?> GetByIdWithDetailsAsync(int id);
}
