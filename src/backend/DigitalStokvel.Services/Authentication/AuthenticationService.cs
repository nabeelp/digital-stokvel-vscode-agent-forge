using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DigitalStokvel.Services.Authentication;

/// <summary>
/// Authentication service for PIN-based login, JWT tokens, and biometric authentication
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly INotificationService _notificationService;

    private const int MaxFailedAttempts = 3;
    private const int LockoutDurationMinutes = 30;
    private const int AccessTokenExpirationMinutes = 15; // SP-14
    private const int RefreshTokenExpirationDays = 7;
    private const int BcryptWorkFactor = 12;

    public AuthenticationService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogRepository auditLogRepository,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger,
        IFraudDetectionService fraudDetectionService,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogRepository = auditLogRepository;
        _configuration = configuration;
        _logger = logger;
        _fraudDetectionService = fraudDetectionService;
        _notificationService = notificationService;
    }

    public async Task<(bool Success, LoginResponse? Response, string? ErrorMessage)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Validate phone number format
            if (!ValidatePhoneNumber(request.PhoneNumber))
            {
                return (false, null, "Invalid phone number format. Must be +27XXXXXXXXX");
            }

            // Validate PIN format
            if (!ValidatePin(request.Pin))
            {
                return (false, null, "Invalid PIN. Must be 4-6 digits without sequential or repeated numbers");
            }

            // Validate ID number (South African ID)
            if (!ValidateIdNumber(request.IdNumber))
            {
                return (false, null, "Invalid South African ID number");
            }

            // Check if phone number already exists
            if (await _userRepository.PhoneNumberExistsAsync(request.PhoneNumber))
            {
                return (false, null, "Phone number already registered");
            }

            // Check if ID number already exists
            if (await _userRepository.IdNumberExistsAsync(request.IdNumber))
            {
                return (false, null, "ID number already registered");
            }

            // Hash PIN with bcrypt
            var pinHash = BCrypt.Net.BCrypt.HashPassword(request.Pin, BcryptWorkFactor);

            // Create user entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                PhoneNumber = request.PhoneNumber,
                PinHash = pinHash,
                FullName = request.FullName,
                IdNumber = request.IdNumber,
                Email = request.Email,
                Status = UserStatus.PendingVerification,
                POPIAConsentAccepted = request.POPIAConsent,
                POPIAConsentAcceptedAt = request.POPIAConsent ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save user
            await _userRepository.CreateAsync(user);

            // TODO: Initiate FICA verification process (mock for MVP)
            // In production, integrate with ID verification service
            await PerformFICAVerificationAsync(user);

            // Log registration
            await LogAuthenticationEventAsync(user.Id.ToString(), "Register", null, null, true);

            _logger.LogInformation("User registered successfully: {PhoneNumber}", request.PhoneNumber);

            // Auto-login after registration
            var loginResponse = await GenerateLoginResponseAsync(user, null, null);
            return (true, loginResponse, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {PhoneNumber}", request.PhoneNumber);
            return (false, null, "Registration failed. Please try again.");
        }
    }

    public async Task<(bool Success, LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request)
    {
        try
        {
            // Get user by phone number
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user == null)
            {
                await LogAuthenticationEventAsync(null, "FailedLogin", request.IpAddress, request.UserAgent, false, "User not found");
                return (false, null, "Invalid phone number or PIN");
            }

            // Check if account is locked
            if (user.IsLocked())
            {
                await LogAuthenticationEventAsync(user.Id.ToString(), "FailedLogin", request.IpAddress, request.UserAgent, false, "Account locked");
                return (false, null, $"Account is locked until {user.LockedUntil:HH:mm}. Please try again later.");
            }

            // Check if account is suspended
            if (user.IsSuspended())
            {
                await LogAuthenticationEventAsync(user.Id.ToString(), "FailedLogin", request.IpAddress, request.UserAgent, false, "Account suspended");
                return (false, null, "Account is suspended. Please contact support.");
            }

            // Verify PIN
            if (!BCrypt.Net.BCrypt.Verify(request.Pin, user.PinHash))
            {
                // Increment failed attempts
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id.ToString());
                user.FailedLoginAttempts++; // Update local object

                // Lock account if max attempts reached (SP-15)
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    var unlockTime = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                    await _userRepository.LockAccountAsync(user.Id.ToString(), unlockTime);
                    
                    // Send SMS notification
                    await _notificationService.SendSmsAsync(
                        user.PhoneNumber,
                        $"Your account has been locked due to multiple failed login attempts. It will be unlocked at {unlockTime:HH:mm}."
                    );

                    await LogAuthenticationEventAsync(user.Id.ToString(), "AccountLocked", request.IpAddress, request.UserAgent, false, "Max failed attempts reached");
                    
                    return (false, null, $"Account locked due to multiple failed attempts. Try again after {unlockTime:HH:mm}.");
                }

                await LogAuthenticationEventAsync(user.Id.ToString(), "FailedLogin", request.IpAddress, request.UserAgent, false, "Invalid PIN");
                
                return (false, null, $"Invalid phone number or PIN. {MaxFailedAttempts - user.FailedLoginAttempts} attempts remaining.");
            }

            // Check for suspicious activity (fraud detection)
            var fraudCheck = await _fraudDetectionService.DetectSuspiciousLoginAsync(user.Id.ToString(), request.IpAddress ?? "", request.DeviceId ?? "");
            if (fraudCheck.IsSuspicious && fraudCheck.RiskScore >= 70)
            {
                // High risk - require 2FA
                await LogAuthenticationEventAsync(user.Id.ToString(), "SuspiciousLogin", request.IpAddress, request.UserAgent, false, $"High risk score: {fraudCheck.RiskScore}");
                
                // TODO: Initiate 2FA flow
                return (false, null, "Suspicious activity detected. Please verify your identity via SMS.");
            }

            // Successful login
            await _userRepository.ResetFailedLoginAttemptsAsync(user.Id.ToString());
            await _userRepository.UpdateLastLoginAsync(user.Id.ToString(), request.IpAddress ?? "", request.DeviceId);

            // Generate tokens
            var loginResponse = await GenerateLoginResponseAsync(user, request.IpAddress, request.DeviceId);

            await LogAuthenticationEventAsync(user.Id.ToString(), "Login", request.IpAddress, request.UserAgent, true);

            _logger.LogInformation("User logged in successfully: {PhoneNumber}", request.PhoneNumber);

            return (true, loginResponse, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {PhoneNumber}", request.PhoneNumber);
            return (false, null, "Login failed. Please try again.");
        }
    }

    public async Task<(bool Success, RefreshTokenResponse? Response, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            // Validate refresh token
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return (false, null, "Invalid or expired refresh token");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null || !user.CanAuthenticate())
            {
                return (false, null, "User account is not active");
            }

            // Revoke old token (token rotation)
            await _refreshTokenRepository.RevokeAsync(request.RefreshToken, null, "Rotated");

            // Generate new tokens
            var accessToken = GenerateAccessToken(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id.ToString(), null, null);

            var response = new RefreshTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = AccessTokenExpirationMinutes * 60
            };

            await LogAuthenticationEventAsync(user.Id.ToString(), "TokenRefresh", null, null, true);

            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return (false, null, "Token refresh failed");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangePinAsync(string userId, ChangePinRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Verify old PIN
            if (!BCrypt.Net.BCrypt.Verify(request.OldPin, user.PinHash))
            {
                await LogAuthenticationEventAsync(userId, "FailedPinChange", null, null, false, "Invalid old PIN");
                return (false, "Invalid current PIN");
            }

            // Validate new PIN
            if (!ValidatePin(request.NewPin))
            {
                return (false, "Invalid PIN. Must be 4-6 digits without sequential or repeated numbers");
            }

            // Check if new PIN is different from old PIN
            if (BCrypt.Net.BCrypt.Verify(request.NewPin, user.PinHash))
            {
                return (false, "New PIN must be different from current PIN");
            }

            // Hash new PIN
            user.PinHash = BCrypt.Net.BCrypt.HashPassword(request.NewPin, BcryptWorkFactor);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Revoke all refresh tokens for security
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "PIN changed");

            await LogAuthenticationEventAsync(userId, "PinChange", null, null, true);

            // Send SMS notification
            await _notificationService.SendSmsAsync(
                user.PhoneNumber,
                "Your PIN was successfully changed. If you did not make this change, please contact support immediately."
            );

            _logger.LogInformation("PIN changed successfully for user: {UserId}", userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PIN change for user: {UserId}", userId);
            return (false, "PIN change failed");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> LogoutAsync(string userId, LogoutRequest request)
    {
        try
        {
            if (request.RevokeAllTokens)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "User logout");
            }
            else
            {
                await _refreshTokenRepository.RevokeAsync(request.RefreshToken, null, "User logout");
            }

            await LogAuthenticationEventAsync(userId, "Logout", null, null, true);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return (false, "Logout failed");
        }
    }

    #region Biometric Authentication

    public async Task<(bool Success, string? ErrorMessage)> BiometricEnrollAsync(string userId, BiometricEnrollRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            user.BiometricEnabled = true;
            user.BiometricPublicKey = request.BiometricPublicKey;
            user.BiometricDeviceId = request.DeviceId;
            user.BiometricPlatform = request.Platform;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            await LogAuthenticationEventAsync(userId, "BiometricEnroll", null, null, true);

            _logger.LogInformation("Biometric enrolled for user: {UserId}", userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric enrollment for user: {UserId}", userId);
            return (false, "Biometric enrollment failed");
        }
    }

    public async Task<(bool Success, LoginResponse? Response, string? ErrorMessage)> BiometricLoginAsync(BiometricLoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user == null || !user.BiometricEnabled)
            {
                return (false, null, "Biometric authentication not enabled");
            }

            // Validate biometric signature
            // TODO: In production, implement proper biometric signature validation
            // For MVP, we'll simulate validation
            if (!ValidateBiometricSignature(request.Challenge, request.BiometricSignature, user.BiometricPublicKey!))
            {
                await LogAuthenticationEventAsync(user.Id.ToString(), "FailedBiometricLogin", null, null, false, "Invalid biometric signature");
                return (false, null, "Biometric authentication failed");
            }

            // Generate tokens
            var loginResponse = await GenerateLoginResponseAsync(user, null, request.DeviceId);

            await LogAuthenticationEventAsync(user.Id.ToString(), "BiometricLogin", null, null, true);

            _logger.LogInformation("User logged in via biometric: {PhoneNumber}", request.PhoneNumber);

            return (true, loginResponse, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric login for {PhoneNumber}", request.PhoneNumber);
            return (false, null, "Biometric login failed");
        }
    }

    #endregion

    #region Private Helper Methods

    private string GenerateAccessToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "DigitalStokvel";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "DigitalStokvelApp";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.MobilePhone, user.PhoneNumber),
            new(ClaimTypes.Name, user.FullName),
            new("phone_number", user.PhoneNumber),
            new("user_id", user.Id.ToString()),
            new("fica_verified", user.FICAVerified.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress, string? deviceId)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            DeviceId = deviceId
        };

        return await _refreshTokenRepository.CreateAsync(token);
    }

    private async Task<LoginResponse> GenerateLoginResponseAsync(User user, string? ipAddress, string? deviceId)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id.ToString(), ipAddress, deviceId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = AccessTokenExpirationMinutes * 60,
            Profile = new UserProfileDto
            {
                UserId = user.Id.ToString(),
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Email = user.Email,
                BiometricEnabled = user.BiometricEnabled,
                TwoFactorEnabled = user.TwoFactorEnabled,
                FICAVerified = user.FICAVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            }
        };
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private bool ValidatePhoneNumber(string phoneNumber)
    {
        // South African phone number format: +27XXXXXXXXX (11 digits total)
        return phoneNumber.StartsWith("+27") && phoneNumber.Length == 12 && phoneNumber[3..].All(char.IsDigit);
    }

    private bool ValidatePin(string pin)
    {
        // 4-6 digits only
        if (pin.Length < 4 || pin.Length > 6 || !pin.All(char.IsDigit))
        {
            return false;
        }

        // No sequential numbers (1234, 4567, etc.)
        for (int i = 0; i < pin.Length - 1; i++)
        {
            if (pin[i] + 1 == pin[i + 1] && (i == pin.Length - 2 || pin[i + 1] + 1 == pin[i + 2]))
            {
                return false;
            }
        }

        // No repeated numbers (1111, 2222, etc.)
        if (pin.All(c => c == pin[0]))
        {
            return false;
        }

        return true;
    }

    private bool ValidateIdNumber(string idNumber)
    {
        // South African ID number: YYMMDD SSSS C A Z (13 digits)
        if (idNumber.Length != 13 || !idNumber.All(char.IsDigit))
        {
            return false;
        }

        // Validate date of birth (first 6 digits)
        if (!int.TryParse(idNumber[..2], out int year) ||
            !int.TryParse(idNumber.Substring(2, 2), out int month) ||
            !int.TryParse(idNumber.Substring(4, 2), out int day))
        {
            return false;
        }

        if (month < 1 || month > 12 || day < 1 || day > 31)
        {
            return false;
        }

        // Validate checksum (Luhn algorithm)
        return ValidateIdNumberChecksum(idNumber);
    }

    private bool ValidateIdNumberChecksum(string idNumber)
    {
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(idNumber[i].ToString());
            if (i % 2 == 0)
            {
                sum += digit;
            }
            else
            {
                int doubled = digit * 2;
                sum += doubled > 9 ? doubled - 9 : doubled;
            }
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == int.Parse(idNumber[12].ToString());
    }

    private bool ValidateBiometricSignature(string challenge, string signature, string publicKey)
    {
        // TODO: Implement proper biometric signature validation
        // For MVP, we'll simulate validation
        // In production, use cryptographic validation with the stored public key
        return !string.IsNullOrEmpty(signature) && !string.IsNullOrEmpty(publicKey);
    }

    private async Task PerformFICAVerificationAsync(User user)
    {
        // TODO: Implement actual FICA verification with ID document upload and selfie verification
        // For MVP, auto-verify for testing
        user.FICAVerified = true;
        user.FICAVerifiedAt = DateTime.UtcNow;
        user.Status = UserStatus.Active;
        await _userRepository.UpdateAsync(user);
    }

    private async Task LogAuthenticationEventAsync(string? userId, string action, string? ipAddress, string? userAgent, bool success, string? failureReason = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = "User",
            EntityId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Success = success,
            FailureReason = failureReason,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.Authentication,
            Severity = success ? AuditSeverity.Information : AuditSeverity.Warning
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    #endregion
}
