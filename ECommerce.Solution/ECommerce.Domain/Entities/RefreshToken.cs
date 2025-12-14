namespace ECommerce.Domain.Entities;

/// <summary>
/// Refresh token for JWT authentication with enhanced security tracking
/// </summary>
public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    
    /// <summary>
    /// Unique refresh token (128-byte cryptographic random, Base64 encoded)
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID (FK to AspNetUsers)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Token expiration date (30 days, sliding)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Token creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Token revocation timestamp (null if active)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Token that replaced this one (rotation chain)
    /// </summary>
    public string? ReplacedByToken { get; set; }
    
    /// <summary>
    /// Whether token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// Whether token has been used (one-time use)
    /// </summary>
    public bool IsUsed { get; set; }
    
    // CRITICAL SECURITY FIELDS
    
    /// <summary>
    /// IP address when token was created (IPv4/IPv6)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent (browser/device fingerprint)
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Optional user-friendly device name
    /// </summary>
    public string? DeviceName { get; set; }
    
    /// <summary>
    /// Last time token was used (for sliding expiration)
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation Properties
    
    /// <summary>
    /// User who owns this token
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
    
    // Computed Properties
    
    /// <summary>
    /// Whether token is currently active (not revoked, not used, not expired)
    /// </summary>
    public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;
}
