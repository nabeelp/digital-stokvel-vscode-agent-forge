namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// User registration request with PIN-based authentication
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Phone number in format +27XXXXXXXXX
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// 4-6 digit PIN for authentication
    /// </summary>
    public required string Pin { get; init; }

    /// <summary>
    /// User's full legal name
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// South African ID number for FICA verification
    /// </summary>
    public required string IdNumber { get; init; }

    /// <summary>
    /// POPIA consent flag
    /// </summary>
    public bool POPIAConsent { get; init; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; init; }
}

/// <summary>
/// Login request with phone number and PIN
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// Phone number in format +27XXXXXXXXX
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// 4-6 digit PIN
    /// </summary>
    public required string Pin { get; init; }

    /// <summary>
    /// Device identifier for fraud detection
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// IP address for audit logging
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// User agent string for audit logging
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Successful login response with tokens
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// JWT access token (15 minute expiry)
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// Refresh token (7 day expiry)
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token expiry time in seconds
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// User profile information
    /// </summary>
    public required UserProfileDto Profile { get; init; }
}

/// <summary>
/// User profile information
/// </summary>
public record UserProfileDto
{
    public required string UserId { get; init; }
    public required string PhoneNumber { get; init; }
    public required string FullName { get; init; }
    public string? Email { get; init; }
    public bool BiometricEnabled { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public bool FICAVerified { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

/// <summary>
/// Refresh token request
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Refresh token issued during login
    /// </summary>
    public required string RefreshToken { get; init; }
}

/// <summary>
/// Refresh token response with new access token
/// </summary>
public record RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// New refresh token (rotated)
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Token expiry time in seconds
    /// </summary>
    public int ExpiresIn { get; init; }
}

/// <summary>
/// PIN change request
/// </summary>
public record ChangePinRequest
{
    /// <summary>
    /// Current PIN for verification
    /// </summary>
    public required string OldPin { get; init; }

    /// <summary>
    /// New 4-6 digit PIN
    /// </summary>
    public required string NewPin { get; init; }
}

/// <summary>
/// Biometric enrollment request
/// </summary>
public record BiometricEnrollRequest
{
    /// <summary>
    /// Unique device identifier
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// Biometric public key for verification
    /// </summary>
    public required string BiometricPublicKey { get; init; }

    /// <summary>
    /// Device platform (iOS/Android)
    /// </summary>
    public required string Platform { get; init; }
}

/// <summary>
/// Biometric login request
/// </summary>
public record BiometricLoginRequest
{
    /// <summary>
    /// Phone number for user identification
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Device identifier
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// Signed challenge with biometric private key
    /// </summary>
    public required string BiometricSignature { get; init; }

    /// <summary>
    /// Challenge that was signed
    /// </summary>
    public required string Challenge { get; init; }
}

/// <summary>
/// Two-factor authentication initiation request
/// </summary>
public record TwoFactorChallengeRequest
{
    /// <summary>
    /// User ID requesting 2FA
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Reason for 2FA (e.g., "Payout approval for R10,000")
    /// </summary>
    public required string Reason { get; init; }
}

/// <summary>
/// Two-factor authentication challenge response
/// </summary>
public record TwoFactorChallengeResponse
{
    /// <summary>
    /// Challenge ID for validation
    /// </summary>
    public required string ChallengeId { get; init; }

    /// <summary>
    /// Phone number where OTP was sent (masked)
    /// </summary>
    public required string MaskedPhoneNumber { get; init; }

    /// <summary>
    /// OTP expiry time
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Two-factor authentication validation request
/// </summary>
public record TwoFactorValidateRequest
{
    /// <summary>
    /// Challenge ID from initiation
    /// </summary>
    public required string ChallengeId { get; init; }

    /// <summary>
    /// 6-digit OTP code
    /// </summary>
    public required string OtpCode { get; init; }
}

/// <summary>
/// Two-factor authentication validation response
/// </summary>
public record TwoFactorValidateResponse
{
    /// <summary>
    /// Validation success flag
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Validation token for subsequent operations
    /// </summary>
    public string? ValidationToken { get; init; }

    /// <summary>
    /// Validation token expiry
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// PIN reset initiation request
/// </summary>
public record PinResetRequest
{
    /// <summary>
    /// Phone number for account identification
    /// </summary>
    public required string PhoneNumber { get; init; }

    /// <summary>
    /// ID number for identity verification
    /// </summary>
    public required string IdNumber { get; init; }
}

/// <summary>
/// PIN reset confirmation request
/// </summary>
public record PinResetConfirmRequest
{
    /// <summary>
    /// Reset token from SMS
    /// </summary>
    public required string ResetToken { get; init; }

    /// <summary>
    /// 6-digit OTP
    /// </summary>
    public required string OtpCode { get; init; }

    /// <summary>
    /// New 4-6 digit PIN
    /// </summary>
    public required string NewPin { get; init; }
}

/// <summary>
/// Logout request
/// </summary>
public record LogoutRequest
{
    /// <summary>
    /// Refresh token to revoke
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Revoke all refresh tokens for this user
    /// </summary>
    public bool RevokeAllTokens { get; init; }
}
