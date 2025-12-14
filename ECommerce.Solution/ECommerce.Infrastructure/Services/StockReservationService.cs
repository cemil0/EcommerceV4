using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Stock reservation service - SIMPLIFIED for MVP
/// No nested transactions - works within OrderService transaction
/// </summary>
public class StockReservationService : IStockReservationService
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<StockReservationService> _logger;

    public StockReservationService(
        ECommerceDbContext context,
        ILogger<StockReservationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StockReservationResult> ReserveStockAsync(
        List<StockReservationItemDto> items,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting stock reservation for {ItemCount} items", items.Count);

        var reservedItems = new List<StockReservationItem>();

        // NO TRANSACTION HERE - Will use OrderService's transaction
        // This ensures atomic operation: if order fails, stock reservation also rolls back

        foreach (var item in items)
        {
            // Pessimistic locking: Lock the row for update
            // Optimized pattern: FromSqlRaw + AsTracking + explicit Load
            var variant = await _context.ProductVariants
                .FromSqlRaw("SELECT * FROM ProductVariants WITH (UPDLOCK, ROWLOCK) WHERE ProductVariantId = {0}", 
                    item.ProductVariantId)
                .AsTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (variant == null)
            {
                _logger.LogWarning("Product variant {VariantId} not found", item.ProductVariantId);
                return StockReservationResult.Failed(
                    $"Product variant {item.ProductVariantId} not found");
            }

            // Load Product navigation property explicitly (better than Include with FromSqlRaw)
            await _context.Entry(variant)
                .Reference(v => v.Product)
                .LoadAsync(cancellationToken);

            // Check available stock (total stock - already reserved)
            var availableStock = variant.StockQuantity - variant.ReservedQuantity;
            
            if (availableStock < item.Quantity)
            {
                _logger.LogWarning(
                    "Insufficient stock for {ProductName}. Available: {Available}, Requested: {Requested}",
                    variant.Product.ProductName, availableStock, item.Quantity);

                return StockReservationResult.Failed(
                    $"Insufficient stock for {variant.Product.ProductName}. " +
                    $"Available: {availableStock}, Requested: {item.Quantity}");
            }

            // Reserve stock (increase reserved quantity)
            variant.ReservedQuantity += item.Quantity;
            variant.UpdatedAt = DateTime.UtcNow;

            reservedItems.Add(new StockReservationItem
            {
                ProductVariantId = item.ProductVariantId,
                ProductName = variant.Product.ProductName,
                QuantityReserved = item.Quantity,
                ReservedAt = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Reserved {Quantity} units of {ProductName}. Stock: {Stock}, Reserved: {Reserved}, Available: {Available}",
                item.Quantity, variant.Product.ProductName, 
                variant.StockQuantity, variant.ReservedQuantity, 
                variant.StockQuantity - variant.ReservedQuantity);
        }

        // Save changes (will be part of OrderService transaction)
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock reservation completed successfully for {ItemCount} items", items.Count);

        return StockReservationResult.Success(reservedItems);
    }
}
