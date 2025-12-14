using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for OrderStatusHistory entity
/// </summary>
public class OrderStatusHistoryRepository : Repository<OrderStatusHistory>, IOrderStatusHistoryRepository
{
    public OrderStatusHistoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get all status history entries for a specific order, ordered by date
    /// </summary>
    public async Task<List<OrderStatusHistory>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Set<OrderStatusHistory>()
            .Where(h => h.OrderId == orderId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get the latest status change for an order
    /// </summary>
    public async Task<OrderStatusHistory?> GetLatestByOrderIdAsync(int orderId)
    {
        return await _context.Set<OrderStatusHistory>()
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .FirstOrDefaultAsync();
    }
}
