using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task<CartItem?> GetByCartAndVariantAsync(int cartId, int productVariantId);
}
