using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
}
