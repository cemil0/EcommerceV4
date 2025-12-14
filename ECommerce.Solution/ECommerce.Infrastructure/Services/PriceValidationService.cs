using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Price validation service - validates cart prices against current database prices
/// </summary>
public class PriceValidationService : IPriceValidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PriceValidationService> _logger;

    public PriceValidationService(
        IUnitOfWork unitOfWork,
        ILogger<PriceValidationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PriceValidationResult> ValidatePricesAsync(
        List<PriceValidationItemDto> items,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating prices for {ItemCount} items", items.Count);

        var priceChanges = new List<PriceChangeDetail>();

        foreach (var item in items)
        {
            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(item.ProductVariantId);
            
            if (variant == null)
            {
                _logger.LogWarning("Product variant {VariantId} not found during price validation", 
                    item.ProductVariantId);
                continue; // Will be caught by stock reservation
            }

            // Get current price (SalePrice if available, otherwise BasePrice)
            var currentPrice = variant.SalePrice ?? variant.BasePrice;

            // Check if price has changed (use decimal precision to avoid floating-point issues)
            var priceDifference = Math.Abs(currentPrice - item.ExpectedPrice);
            if (priceDifference >= 0.01m) // 1 cent tolerance
            {
                _logger.LogWarning(
                    "Price mismatch for variant {VariantId}. Expected: {Expected}, Current: {Current}, Difference: {Difference}",
                    variant.ProductVariantId, item.ExpectedPrice, currentPrice, priceDifference);

                priceChanges.Add(new PriceChangeDetail
                {
                    ProductVariantId = variant.ProductVariantId,
                    ProductName = variant.Product?.ProductName ?? "Unknown Product",
                    ExpectedPrice = item.ExpectedPrice,
                    CurrentPrice = currentPrice
                });
            }
        }

        if (priceChanges.Any())
        {
            _logger.LogWarning("Price validation failed. {ChangeCount} price changes detected", 
                priceChanges.Count);
            return PriceValidationResult.Invalid(priceChanges);
        }

        _logger.LogInformation("Price validation passed for all {ItemCount} items", items.Count);
        return PriceValidationResult.Valid();
    }
}
