using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;

namespace ECommerce.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CacheTestController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cache;
    private readonly ILogger<CacheTestController> _logger;

    public CacheTestController(
        IProductService productService,
        IMemoryCache memoryCache,
        ICacheService cache,
        ILogger<CacheTestController> logger)
    {
        _productService = productService;
        _memoryCache = memoryCache;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Get cache metrics (hit rate, total requests, errors)
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get cache metrics", Description = "Returns Redis cache performance metrics")]
    public IActionResult GetCacheMetrics()
    {
        var metrics = _cache.GetMetrics();
        
        return Ok(new
        {
            metrics.CacheHits,
            metrics.CacheMisses,
            metrics.CacheErrors,
            HitRate = $"{metrics.HitRate:P2}",
            metrics.TotalRequests,
            Status = metrics.HitRate > 0.8 ? "Excellent ✅" : 
                     metrics.HitRate > 0.6 ? "Good 👍" : 
                     metrics.HitRate > 0.4 ? "Fair ⚠️" : "Poor ❌",
            CacheType = "Redis Distributed Cache",
            Features = new[]
            {
                "Stampede Protection (Distributed Locking)",
                "LZ4 Compression (40-60% savings)",
                "Namespace Isolation (ECommerce:*)",
                "Metrics Tracking"
            }
        });
    }

    /// <summary>
    /// Test Redis connection and performance
    /// </summary>
    [HttpGet("redis-test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Test Redis connection", Description = "Tests Redis connection and basic operations")]
    public async Task<IActionResult> TestRedis()
    {
        var testKey = "test:redis:connection";
        var testValue = new { Message = "Redis is working!", Timestamp = DateTime.UtcNow };
        
        var sw = Stopwatch.StartNew();
        
        // Set
        await _cache.SetAsync(testKey, testValue, TimeSpan.FromMinutes(1));
        var setTime = sw.ElapsedMilliseconds;
        
        sw.Restart();
        
        // Get
        var retrieved = await _cache.GetAsync<object>(testKey);
        var getTime = sw.ElapsedMilliseconds;
        
        return Ok(new
        {
            Success = retrieved != null,
            SetTimeMs = setTime,
            GetTimeMs = getTime,
            Original = testValue,
            Retrieved = retrieved
        });
    }

    /// <summary>
    /// Test product cache performance
    /// </summary>
    [HttpGet("product-cache-test/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Test product cache", Description = "Compares cache hit vs cache miss performance")]
    public async Task<IActionResult> TestProductCache(int id)
    {
        var sw = Stopwatch.StartNew();
        
        // First call (might be cache miss)
        var product1 = await _productService.GetByIdAsync(id);
        var time1 = sw.ElapsedMilliseconds;
        
        sw.Restart();
        
        // Second call (should be cache hit)
        var product2 = await _productService.GetByIdAsync(id);
        var time2 = sw.ElapsedMilliseconds;
        
        return Ok(new
        {
            ProductId = id,
            FirstCallMs = time1,
            SecondCallMs = time2,
            SpeedupFactor = time1 > 0 ? $"{(double)time1 / time2:F1}x" : "N/A",
            CacheWorking = time2 < time1,
            Performance = time2 < 10 ? "Excellent ✅" : 
                         time2 < 50 ? "Good 👍" : "Slow ⚠️"
        });
    }
}
