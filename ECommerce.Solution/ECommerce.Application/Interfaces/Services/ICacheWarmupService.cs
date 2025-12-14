namespace ECommerce.Application.Interfaces.Services;

/// <summary>
/// Service for warming up cache on application startup
/// </summary>
public interface ICacheWarmupService
{
    /// <summary>
    /// Warm up cache with frequently accessed data
    /// </summary>
    Task WarmupAsync(CancellationToken cancellationToken = default);
}
