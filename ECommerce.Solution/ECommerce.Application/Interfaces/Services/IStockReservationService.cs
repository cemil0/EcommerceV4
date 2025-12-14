using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Stock reservation service interface - SIMPLIFIED for MVP
/// </summary>
public interface IStockReservationService
{
    /// <summary>
    /// Reserves stock for order items with pessimistic locking.
    /// Must be called within an active transaction (OrderService transaction).
    /// </summary>
    Task<StockReservationResult> ReserveStockAsync(
        List<StockReservationItemDto> items,
        CancellationToken cancellationToken = default);
}
