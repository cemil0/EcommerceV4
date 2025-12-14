using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for OrderStatusHistory entity
/// Provides methods to track order status changes for audit trail
/// </summary>
public interface IOrderStatusHistoryRepository : IRepository<OrderStatusHistory>
{
    /// <summary>
    /// Get all status history entries for a specific order
    /// </summary>
    Task<List<OrderStatusHistory>> GetByOrderIdAsync(int orderId);
    
    /// <summary>
    /// Get the latest status change for an order
    /// </summary>
    Task<OrderStatusHistory?> GetLatestByOrderIdAsync(int orderId);
}
