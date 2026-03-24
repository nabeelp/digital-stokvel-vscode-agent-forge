using DigitalStokvel.Core.Entities;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for RefreshToken entity
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Get refresh token by token value
    /// </summary>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Get all active tokens for a user
    /// </summary>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>
    /// Create new refresh token
    /// </summary>
    Task<RefreshToken> CreateAsync(RefreshToken token);

    /// <summary>
    /// Revoke refresh token
    /// </summary>
    Task<bool> RevokeAsync(string token, string? revokedByIp, string? reason);

    /// <summary>
    /// Revoke all tokens for a user
    /// </summary>
    Task<int> RevokeAllUserTokensAsync(string userId, string? reason);

    /// <summary>
    /// Delete expired tokens
    /// </summary>
    Task<int> DeleteExpiredTokensAsync();

    /// <summary>
    /// Check if token exists and is active
    /// </summary>
    Task<bool> IsTokenActiveAsync(string token);
}
