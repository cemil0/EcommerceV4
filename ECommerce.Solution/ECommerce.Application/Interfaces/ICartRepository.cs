using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(int customerId);
    Task<Cart?> GetBySessionIdAsync(string sessionId);
    Task<Cart?> GetActiveCartWithItemsAsync(int customerId);
}
