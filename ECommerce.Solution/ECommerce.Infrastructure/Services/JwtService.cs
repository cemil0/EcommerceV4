using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ECommerceDbContext _context;
    private readonly ILogger<JwtService> _logger;
    private const int MAX_ACTIVE_TOKENS = 5;

    public JwtService(
        IConfiguration configuration,
        IRefreshTokenRepository refreshTokenRepository,
        ECommerceDbContext context,
        ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _logger = logger;
    }

    public string GenerateToken(string userId, string email, IList<string> roles, int? customerId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add customerId claim if provided
        if (customerId.HasValue)
        {
            claims.Add(new Claim("CustomerId", customerId.Value.ToString()));
        }

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:ExpirationDays"] ?? "7")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "");

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    // REFRESH TOKEN METHODS

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[128]; // 128 bytes = 172 chars Base64
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(
        string userId,
        string ipAddress,
        string userAgent,
        string? deviceName = null)
    {
        // Check device limit
        var activeCount = await _refreshTokenRepository.GetActiveTokenCountAsync(userId);
        if (activeCount >= MAX_ACTIVE_TOKENS)
        {
            _logger.LogWarning("User {UserId} reached device limit ({Limit}). Revoking oldest token.", userId, MAX_ACTIVE_TOKENS);
            await _refreshTokenRepository.RevokeOldestTokenAsync(userId);
        }

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 days
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = deviceName,
            LastUsedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        
        _logger.LogInformation("Created refresh token for user {UserId} from IP {IpAddress}", userId, ipAddress);
        
        return refreshToken;
    }

    public async Task<bool> ValidateRefreshTokenAsync(
        string token,
        string ipAddress,
        string userAgent)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            return false;
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Refresh token {Token} is revoked", token.Substring(0, 10) + "...");
            return false;
        }

        if (refreshToken.IsUsed)
        {
            _logger.LogWarning("Refresh token {Token} is already used", token.Substring(0, 10) + "...");
            return false;
        }

        if (DateTime.UtcNow >= refreshToken.ExpiresAt)
        {
            _logger.LogWarning("Refresh token {Token} is expired", token.Substring(0, 10) + "...");
            return false;
        }

        // Anomaly detection (warning only, not rejection)
        if (refreshToken.IpAddress != ipAddress)
        {
            _logger.LogWarning(
                "IP mismatch for token. Original: {Original}, Current: {Current}, User: {UserId}",
                refreshToken.IpAddress,
                ipAddress,
                refreshToken.UserId);
        }

        if (refreshToken.UserAgent != userAgent)
        {
            _logger.LogWarning(
                "UserAgent mismatch for token. User: {UserId}",
                refreshToken.UserId);
        }

        return true;
    }

    public async Task<RefreshToken> RefreshTokenAsync(
        string token,
        string ipAddress,
        string userAgent)
    {
        // Use transaction for atomic update
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var oldToken = await _refreshTokenRepository.GetByTokenAsync(token);
            if (oldToken == null)
                throw new SecurityException("Invalid refresh token");

            // Mark old token as used
            oldToken.IsUsed = true;
            oldToken.LastUsedAt = DateTime.UtcNow;

            // Generate new token with sliding expiration
            var newToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                UserId = oldToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // SLIDING: Reset to 30 days
                CreatedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceName = oldToken.DeviceName,
                LastUsedAt = DateTime.UtcNow
            };

            // Link tokens (rotation chain)
            oldToken.ReplacedByToken = newToken.Token;

            await _refreshTokenRepository.UpdateAsync(oldToken);
            await _refreshTokenRepository.AddAsync(newToken);

            await transaction.CommitAsync();

            _logger.LogInformation("Refreshed token for user {UserId}", oldToken.UserId);

            return newToken;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(refreshToken);
            
            _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        _logger.LogInformation("Revoked all tokens for user {UserId}", userId);
    }
}
