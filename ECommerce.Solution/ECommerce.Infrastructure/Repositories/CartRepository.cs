using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<Cart?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);
    }

    public async Task<Cart?> GetActiveCartWithItemsAsync(int customerId)
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }
}
