using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RefreshToken entity
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DigitalStokvelDbContext _context;

    public RefreshTokenRepository(DigitalStokvelDbContext context)
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
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsExpired)
            .ToListAsync();
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<bool> RevokeAsync(string token, string? revokedByIp, string? reason)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken == null || refreshToken.IsRevoked) return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = revokedByIp;
        refreshToken.RevocationReason = reason;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> RevokeAllUserTokensAsync(string userId, string? reason)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevocationReason = reason;
        }

        await _context.SaveChangesAsync();
        return tokens.Count;
    }

    public async Task<int> DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.RevokedAt < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
        return expiredTokens.Count;
    }

    public async Task<bool> IsTokenActiveAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        return refreshToken?.IsActive ?? false;
    }
}
