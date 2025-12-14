namespace ECommerce.Application.Configuration;

/// <summary>
/// Cache configuration options
/// </summary>
public class CacheOptions
{
    public int DefaultExpirationMinutes { get; set; } = 10;
    public int ProductCacheMinutes { get; set; } = 15;
    public int CategoryCacheMinutes { get; set; } = 30;
    public int UserCacheMinutes { get; set; } = 5;
    public bool EnableCaching { get; set; } = true;
    public bool EnableCompression { get; set; } = true;
    public int CompressionThresholdBytes { get; set; } = 1024; // 1KB
}
