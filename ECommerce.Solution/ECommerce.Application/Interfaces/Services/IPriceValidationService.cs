using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Service for validating product prices before order creation
/// </summary>
public interface IPriceValidationService
{
    /// <summary>
    /// Validates that expected prices match current database prices.
    /// Must be called within OrderService transaction.
    /// </summary>
    Task<PriceValidationResult> ValidatePricesAsync(
        List<PriceValidationItemDto> items,
        CancellationToken cancellationToken = default);
}
