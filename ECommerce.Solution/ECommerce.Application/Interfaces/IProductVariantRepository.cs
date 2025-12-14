using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<ProductVariant?> GetBySKUAsync(string sku);
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
}
