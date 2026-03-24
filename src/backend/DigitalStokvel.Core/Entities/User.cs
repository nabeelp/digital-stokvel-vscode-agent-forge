namespace DigitalStokvel.Core.Entities;

/// <summary>
/// User entity for authentication and profile management
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Phone number in format +27XXXXXXXXX (unique identifier)
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Hashed PIN using bcrypt (work factor 12)
    /// </summary>
    public required string PinHash { get; set; }

    /// <summary>
    /// User's full legal name
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// South African ID number for FICA verification
    /// </summary>
    public required string IdNumber { get; set; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Account status
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Failed login attempt counter (reset after successful login)
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Account locked until this timestamp (null if not locked)
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Biometric authentication enabled flag
    /// </summary>
    public bool BiometricEnabled { get; set; }

    /// <summary>
    /// Biometric public key for signature verification
    /// </summary>
    public string? BiometricPublicKey { get; set; }

    /// <summary>
    /// Device ID for biometric authentication
    /// </summary>
    public string? BiometricDeviceId { get; set; }

    /// <summary>
    /// Biometric platform (iOS/Android)
    /// </summary>
    public string? BiometricPlatform { get; set; }

    /// <summary>
    /// Two-factor authentication enabled flag
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// FICA/KYC verification status
    /// </summary>
    public bool FICAVerified { get; set; }

    /// <summary>
    /// FICA verification date
    /// </summary>
    public DateTime? FICAVerifiedAt { get; set; }

    /// <summary>
    /// POPIA consent accepted flag
    /// </summary>
    public bool POPIAConsentAccepted { get; set; }

    /// <summary>
    /// POPIA consent acceptance date
    /// </summary>
    public DateTime? POPIAConsentAcceptedAt { get; set; }

    /// <summary>
    /// Last successful login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Last login IP address
    /// </summary>
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// Last login device ID
    /// </summary>
    public string? LastLoginDeviceId { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deleted timestamp (for POPIA compliance)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Refresh tokens issued to this user
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Audit logs for this user
    /// </summary>
    public ICollection<AuditLog> AuditLogs { get; set; } = [];

    /// <summary>
    /// Group memberships
    /// </summary>
    public ICollection<Member> GroupMemberships { get; set; } = [];

    /// <summary>
    /// Check if account is currently locked
    /// </summary>
    public bool IsLocked()
    {
        return Status == UserStatus.Locked && 
               LockedUntil.HasValue && 
               LockedUntil.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Check if account is suspended
    /// </summary>
    public bool IsSuspended()
    {
        return Status == UserStatus.Suspended;
    }

    /// <summary>
    /// Check if account can authenticate
    /// </summary>
    public bool CanAuthenticate()
    {
        return Status == UserStatus.Active && 
               !IsLocked() && 
               !IsSuspended() && 
               !IsDeleted;
    }
}

/// <summary>
/// User account status enumeration
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Active account
    /// </summary>
    Active = 1,

    /// <summary>
    /// Temporarily locked due to failed login attempts
    /// </summary>
    Locked = 2,

    /// <summary>
    /// Suspended by administrator or compliance
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Pending FICA verification
    /// </summary>
    PendingVerification = 4
}
