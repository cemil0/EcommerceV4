namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Distributed cache service interface for Redis operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set a value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove a cached value by key
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Remove all keys matching a prefix pattern
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get multiple cached values by keys
    /// </summary>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get cached value or set it using factory function (cache-aside pattern)
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh the expiration time of a cached key
    /// </summary>
    Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get cache performance metrics
    /// </summary>
    CacheMetrics GetMetrics();
}
