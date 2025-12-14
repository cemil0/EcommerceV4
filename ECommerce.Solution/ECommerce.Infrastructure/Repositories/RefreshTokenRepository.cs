using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ECommerceDbContext _context;

    public RefreshTokenRepository(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetActiveTokenCountAsync(string userId)
    {
        return await _context.RefreshTokens
            .CountAsync(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > DateTime.UtcNow);
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RevokeOldestTokenAsync(string userId)
    {
        var oldestToken = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
            .OrderBy(rt => rt.CreatedAt)
            .FirstOrDefaultAsync();

        if (oldestToken != null)
        {
            oldestToken.IsRevoked = true;
            oldestToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
