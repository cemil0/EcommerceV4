namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Cache performance metrics
/// </summary>
public class CacheMetrics
{
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public long CacheErrors { get; set; }
    public double HitRate { get; set; }
    public long TotalRequests { get; set; }
}
