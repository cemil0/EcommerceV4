using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// Repository interface for RefreshToken operations
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Get refresh token by token string
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Get all active tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    
    /// <summary>
    /// Get count of active tokens for a user (for device limit check)
    /// </summary>
    Task<int> GetActiveTokenCountAsync(string userId);
    
    /// <summary>
    /// Add a new refresh token
    /// </summary>
    Task AddAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Update an existing refresh token
    /// </summary>
    Task UpdateAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Revoke all tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(string userId);
    
    /// <summary>
    /// Revoke oldest token for a user (for device limit enforcement)
    /// </summary>
    Task RevokeOldestTokenAsync(string userId);
}
