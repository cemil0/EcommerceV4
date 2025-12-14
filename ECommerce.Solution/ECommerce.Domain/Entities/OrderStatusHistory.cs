using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Tracks order status changes for audit trail
/// </summary>
public class OrderStatusHistory
{
    public int OrderStatusHistoryId { get; set; }
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public int? ChangedByUserId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}
