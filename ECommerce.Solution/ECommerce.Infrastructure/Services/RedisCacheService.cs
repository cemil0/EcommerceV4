using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using K4os.Compression.LZ4;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Compression settings
    private readonly bool _enableCompression = true;
    private readonly int _compressionThreshold = 1024; // 1KB
    
    // Metrics tracking (thread-safe)
    private long _cacheHits = 0;
    private long _cacheMisses = 0;
    private long _cacheErrors = 0;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
            {
                Interlocked.Increment(ref _cacheMisses);
                return default;
            }

            Interlocked.Increment(ref _cacheHits);
            
            // Try to deserialize as CacheEntry (with compression support)
            try
            {
                var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(value!);
                if (cacheEntry != null)
                {
                    byte[] data;
                    if (cacheEntry.IsCompressed)
                    {
                        data = LZ4Pickler.Unpickle(cacheEntry.Data);
                    }
                    else
                    {
                        data = cacheEntry.Data;
                    }

                    var json = Encoding.UTF8.GetString(data);
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
            }
            catch
            {
                // Fallback: try direct deserialization (backward compatibility)
                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }

            return default;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _cacheErrors);
            _logger.LogError(ex, "Error getting cache key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            byte[] dataToStore;
            bool isCompressed = false;

            // Compress if larger than threshold
            if (_enableCompression && bytes.Length > _compressionThreshold)
            {
                var compressed = LZ4Pickler.Pickle(bytes);
                
                if (compressed.Length < bytes.Length * 0.9) // Only if 10%+ savings
                {
                    dataToStore = compressed;
                    isCompressed = true;
                    
                    _logger.LogDebug(
                        "Compressed cache key {Key}: {Original}KB → {Compressed}KB ({Ratio:P0})",
                        key,
                        bytes.Length / 1024.0,
                        compressed.Length / 1024.0,
                        (double)compressed.Length / bytes.Length
                    );
                }
                else
                {
                    dataToStore = bytes;
                }
            }
            else
            {
                dataToStore = bytes;
            }

            // Store with compression flag
            var cacheValue = new CacheEntry
            {
                Data = dataToStore,
                IsCompressed = isCompressed
            };

            var cacheJson = JsonSerializer.Serialize(cacheValue);
            await _db.StringSetAsync(key, cacheJson, expiration);
            
            _logger.LogDebug("Cache set: {Key}, Expiration: {Expiration}, Compressed: {Compressed}", 
                key, expiration, isCompressed);
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _cacheErrors);
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.KeyDeleteAsync(key);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());
            
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            
            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys);
                _logger.LogInformation("Removed {Count} keys with prefix: {Prefix}", keys.Length, prefix);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing keys by prefix: {Prefix}", prefix);
        }
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();
        
        try
        {
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            var values = await _db.StringGetAsync(redisKeys);
            
            for (int i = 0; i < redisKeys.Length; i++)
            {
                if (!values[i].IsNullOrEmpty)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(values[i]!, _jsonOptions);
                    result[redisKeys[i]!] = deserialized;
                }
                else
                {
                    result[redisKeys[i]!] = default;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache keys");
        }
        
        return result;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        // Try get from cache (fast path)
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {Key}", key);

        // Distributed lock to prevent stampede
        var lockKey = $"lock:{key}";
        var lockValue = Guid.NewGuid().ToString();
        var lockExpiry = TimeSpan.FromSeconds(30);

        // Try acquire lock
        var acquired = await _db.StringSetAsync(
            lockKey, 
            lockValue, 
            lockExpiry, 
            When.NotExists
        );

        if (acquired)
        {
            try
            {
                // Double-check cache
                cached = await GetAsync<T>(key, cancellationToken);
                if (cached != null)
                {
                    _logger.LogDebug("Cache HIT after lock acquisition: {Key}", key);
                    return cached;
                }

                _logger.LogDebug("Loading from source with lock: {Key}", key);
                
                var value = await factory();
                await SetAsync(key, value, expiration, cancellationToken);
                
                return value;
            }
            finally
            {
                // Release lock using Lua script
                var script = @"
                    if redis.call('get', KEYS[1]) == ARGV[1] then
                        return redis.call('del', KEYS[1])
                    else
                        return 0
                    end";
                
                try
                {
                    await _db.ScriptEvaluateAsync(
                        script, 
                        new RedisKey[] { lockKey }, 
                        new RedisValue[] { lockValue }
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release lock: {LockKey}", lockKey);
                }
            }
        }
        else
        {
            // Wait for lock holder
            await Task.Delay(100, cancellationToken);
            
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;

            // Retry with backoff
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(100 * (i + 1), cancellationToken);
                cached = await GetAsync<T>(key, cancellationToken);
                if (cached != null)
                    return cached;
            }

            // Fallback
            var value = await factory();
            await SetAsync(key, value, expiration, cancellationToken);
            return value;
        }
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var ttl = await _db.KeyTimeToLiveAsync(key);
            if (ttl.HasValue)
            {
                await _db.KeyExpireAsync(key, ttl.Value);
                _logger.LogDebug("Cache refreshed: {Key}, TTL: {TTL}", key, ttl.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache key: {Key}", key);
        }
    }

    // Metrics
    public CacheMetrics GetMetrics()
    {
        var total = _cacheHits + _cacheMisses;
        
        return new CacheMetrics
        {
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            CacheErrors = _cacheErrors,
            HitRate = total > 0 ? (double)_cacheHits / total : 0,
            TotalRequests = total
        };
    }

    // Helper class for compression
    private class CacheEntry
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public bool IsCompressed { get; set; }
    }
}
