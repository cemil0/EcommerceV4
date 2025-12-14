namespace ECommerce.Application.Configuration;

/// <summary>
/// Rate limiting configuration options
/// </summary>
public class RateLimitingOptions
{
    public Dictionary<string, PolicyOptions> Policies { get; set; } = new();
    public List<string> WhitelistedIPs { get; set; } = new();
    public List<string> BlacklistedIPs { get; set; } = new();
}

/// <summary>
/// Individual rate limit policy configuration
/// </summary>
public class PolicyOptions
{
    public int PermitLimit { get; set; }
    public int WindowMinutes { get; set; }
    public int? TokensPerPeriod { get; set; } // For token bucket algorithm
    public int QueueLimit { get; set; } = 0;
}
