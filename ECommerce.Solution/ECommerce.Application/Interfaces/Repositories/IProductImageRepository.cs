using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IProductImageRepository : IRepository<ProductImage>
{
    Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
    Task<ProductImage?> GetPrimaryImageAsync(int productId);
    Task SetPrimaryImageAsync(int productId, int imageId);
}
