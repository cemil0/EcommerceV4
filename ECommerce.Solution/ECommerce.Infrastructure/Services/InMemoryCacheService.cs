#nullable disable
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly ConcurrentBag<string> _keys = new(); // Track keys for RemoveByPrefix

    // Metrics tracking
    private long _cacheHits = 0;
    private long _cacheMisses = 0;

    public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T value))
        {
            Interlocked.Increment(ref _cacheHits);
            return Task.FromResult<T?>(value);
        }

        Interlocked.Increment(ref _cacheMisses);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        _memoryCache.Set(key, value, options);
        _keys.Add(key); // Naive tracking
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        foreach (var key in _keys.ToArray())
        {
            if (key.StartsWith(prefix))
            {
                _memoryCache.Remove(key);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                result[key] = value;
            }
            else
            {
                result[key] = default;
            }
        }
        // Force cast to interface return type using object first if needed, but dictionary variance might handle it or just copying.
        // IDictionary is invariant. We need precise type match.
        return Task.FromResult<IDictionary<string, T?>>(result);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            Interlocked.Increment(ref _cacheHits);
            return cachedValue;
        }

        Interlocked.Increment(ref _cacheMisses);
        
        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out _);
        return Task.CompletedTask;
    }

    public CacheMetrics GetMetrics()
    {
        var total = _cacheHits + _cacheMisses;
        return new CacheMetrics
        {
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            CacheErrors = 0,
            HitRate = total > 0 ? (double)_cacheHits / total : 0,
            TotalRequests = total
        };
    }
}
