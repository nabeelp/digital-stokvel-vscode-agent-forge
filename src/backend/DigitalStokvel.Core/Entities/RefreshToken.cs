namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Refresh token entity for JWT token rotation
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// User ID this token belongs to
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Refresh token value (cryptographically secure random)
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Token expiration timestamp (7 days from creation)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// IP address where token was created
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Device ID where token was created
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Token revocation timestamp
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP address where token was revoked
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Token replaced by this new token (for rotation)
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Revocation reason
    /// </summary>
    public string? RevocationReason { get; set; }

    // Navigation property
    /// <summary>
    /// User who owns this token
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Check if token is currently valid
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Check if token is revoked
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Check if token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
