using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface IJwtService
{
    // Access Token Methods
    string GenerateToken(string userId, string email, IList<string> roles, int? customerId = null);
    bool ValidateToken(string token);
    
    // Refresh Token Methods
    string GenerateRefreshToken();
    Task<RefreshToken> CreateRefreshTokenAsync(string userId, string ipAddress, string userAgent, string? deviceName = null);
    Task<bool> ValidateRefreshTokenAsync(string token, string ipAddress, string userAgent);
    Task<RefreshToken> RefreshTokenAsync(string token, string ipAddress, string userAgent);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
}
