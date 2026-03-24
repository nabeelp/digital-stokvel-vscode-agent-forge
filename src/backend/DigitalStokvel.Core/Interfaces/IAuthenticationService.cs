namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    Task<(bool Success, DTOs.LoginResponse? Response, string? ErrorMessage)> RegisterAsync(DTOs.RegisterRequest request);
    Task<(bool Success, DTOs.LoginResponse? Response, string? ErrorMessage)> LoginAsync(DTOs.LoginRequest request);
    Task<(bool Success, DTOs.RefreshTokenResponse? Response, string? ErrorMessage)> RefreshTokenAsync(DTOs.RefreshTokenRequest request);
    Task<(bool Success, string? ErrorMessage)> ChangePinAsync(string userId, DTOs.ChangePinRequest request);
    Task<(bool Success, string? ErrorMessage)> LogoutAsync(string userId, DTOs.LogoutRequest request);
    Task<(bool Success, string? ErrorMessage)> BiometricEnrollAsync(string userId, DTOs.BiometricEnrollRequest request);
    Task<(bool Success, DTOs.LoginResponse? Response, string? ErrorMessage)> BiometricLoginAsync(DTOs.BiometricLoginRequest request);
}
