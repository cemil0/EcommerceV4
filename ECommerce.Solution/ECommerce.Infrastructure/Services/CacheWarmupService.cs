using System.Diagnostics;
using ECommerce.Application.Constants;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class CacheWarmupService : ICacheWarmupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<CacheWarmupService> _logger;

    public CacheWarmupService(
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<CacheWarmupService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔥 Starting cache warmup...");
        var sw = Stopwatch.StartNew();

        try
        {
            // Warmup categories (small, frequently accessed)
            await WarmupCategoriesAsync(cancellationToken);
            
            // Warmup featured products
            await WarmupFeaturedProductsAsync(cancellationToken);

            sw.Stop();
            _logger.LogInformation("✅ Cache warmup completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Cache warmup failed");
        }
    }

    private async Task WarmupCategoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetRootCategoriesAsync();
            
            if (categories.Any())
            {
                await _cache.SetAsync(
                    CacheKeys.CategoryList(),
                    categories,
                    TimeSpan.FromMinutes(30),
                    cancellationToken
                );

                _logger.LogInformation("  ✓ Warmed up {Count} categories", categories.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "  ⚠ Failed to warm up categories");
        }
    }

    private async Task WarmupFeaturedProductsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var featuredProducts = await _unitOfWork.Products.GetFeaturedProductsAsync(10);
            
            if (featuredProducts.Any())
            {
                await _cache.SetAsync(
                    $"{CacheKeys.ProductPrefix}Featured:10",
                    featuredProducts,
                    TimeSpan.FromMinutes(15),
                    cancellationToken
                );

                _logger.LogInformation("  ✓ Warmed up {Count} featured products", featuredProducts.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "  ⚠ Failed to warm up featured products");
        }
    }
}
