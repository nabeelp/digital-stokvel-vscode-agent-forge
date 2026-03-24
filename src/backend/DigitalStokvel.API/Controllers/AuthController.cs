using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalStokvel.API.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and PIN management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user with PIN-based authentication
    /// </summary>
    /// <param name="request">Registration request with phone number, PIN, and FICA details</param>
    /// <returns>Login response with JWT tokens</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for phone number: {PhoneNumber}", request.PhoneNumber);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var (success, response, errorMessage) = await _authenticationService.RegisterAsync(request);

        if (!success)
        {
            _logger.LogWarning("Registration failed for {PhoneNumber}: {Error}", request.PhoneNumber, errorMessage);
            return BadRequest(new ErrorResponse { Message = errorMessage ?? "Registration failed" });
        }

        _logger.LogInformation("User registered successfully: {PhoneNumber}", request.PhoneNumber);
        return Ok(response);
    }

    /// <summary>
    /// Login with phone number and PIN
    /// </summary>
    /// <param name="request">Login request with phone number and PIN</param>
    /// <returns>Login response with JWT tokens</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for phone number: {PhoneNumber}", request.PhoneNumber);

        // Capture IP address and user agent for fraud detection
        request = request with
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var (success, response, errorMessage) = await _authenticationService.LoginAsync(request);

        if (!success)
        {
            _logger.LogWarning("Login failed for {PhoneNumber}: {Error}", request.PhoneNumber, errorMessage);
            return Unauthorized(new ErrorResponse { Message = errorMessage ?? "Login failed" });
        }

        _logger.LogInformation("User logged in successfully: {PhoneNumber}", request.PhoneNumber);
        return Ok(response);
    }

    /// <summary>
    /// Refresh JWT access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        var (success, response, errorMessage) = await _authenticationService.RefreshTokenAsync(request);

        if (!success)
        {
            _logger.LogWarning("Token refresh failed: {Error}", errorMessage);
            return BadRequest(new ErrorResponse { Message = errorMessage ?? "Token refresh failed" });
        }

        _logger.LogInformation("Token refreshed successfully");
        return Ok(response);
    }

    /// <summary>
    /// Change user PIN
    /// </summary>
    /// <param name="request">Change PIN request with old and new PINs</param>
    /// <returns>Success response</returns>
    [HttpPut("pin")]
    [Authorize]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePin([FromBody] ChangePinRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ErrorResponse { Message = "User not authenticated" });
        }

        _logger.LogInformation("PIN change attempt for user: {UserId}", userId);

        var (success, errorMessage) = await _authenticationService.ChangePinAsync(userId, request);

        if (!success)
        {
            _logger.LogWarning("PIN change failed for user {UserId}: {Error}", userId, errorMessage);
            return BadRequest(new ErrorResponse { Message = errorMessage ?? "PIN change failed" });
        }

        _logger.LogInformation("PIN changed successfully for user: {UserId}", userId);
        return Ok(new SuccessResponse { Message = "PIN changed successfully. Please login again with your new PIN." });
    }

    /// <summary>
    /// Logout and revoke refresh tokens
    /// </summary>
    /// <param name="request">Logout request with optional flag to revoke all tokens</param>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ErrorResponse { Message = "User not authenticated" });
        }

        _logger.LogInformation("Logout attempt for user: {UserId}", userId);

        var (success, errorMessage) = await _authenticationService.LogoutAsync(userId, request);

        if (!success)
        {
            _logger.LogWarning("Logout failed for user {UserId}: {Error}", userId, errorMessage);
            return BadRequest(new ErrorResponse { Message = errorMessage ?? "Logout failed" });
        }

        _logger.LogInformation("User logged out successfully: {UserId}", userId);
        return Ok(new SuccessResponse { Message = "Logged out successfully" });
    }

    /// <summary>
    /// Enroll biometric authentication
    /// </summary>
    /// <param name="request">Biometric enrollment request with device ID and public key</param>
    /// <returns>Success response</returns>
    [HttpPost("biometric/enroll")]
    [Authorize]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BiometricEnroll([FromBody] BiometricEnrollRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ErrorResponse { Message = "User not authenticated" });
        }

        _logger.LogInformation("Biometric enrollment attempt for user: {UserId}", userId);

        var (success, errorMessage) = await _authenticationService.BiometricEnrollAsync(userId, request);

        if (!success)
        {
            _logger.LogWarning("Biometric enrollment failed for user {UserId}: {Error}", userId, errorMessage);
            return BadRequest(new ErrorResponse { Message = errorMessage ?? "Biometric enrollment failed" });
        }

        _logger.LogInformation("Biometric enrolled successfully for user: {UserId}", userId);
        return Ok(new SuccessResponse { Message = "Biometric authentication enabled successfully" });
    }

    /// <summary>
    /// Login with biometric authentication
    /// </summary>
    /// <param name="request">Biometric login request with signature and challenge</param>
    /// <returns>Login response with JWT tokens</returns>
    [HttpPost("biometric/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BiometricLogin([FromBody] BiometricLoginRequest request)
    {
        _logger.LogInformation("Biometric login attempt for phone number: {PhoneNumber}", request.PhoneNumber);

        var (success, response, errorMessage) = await _authenticationService.BiometricLoginAsync(request);

        if (!success)
        {
            _logger.LogWarning("Biometric login failed for {PhoneNumber}: {Error}", request.PhoneNumber, errorMessage);
            return Unauthorized(new ErrorResponse { Message = errorMessage ?? "Biometric login failed" });
        }

        _logger.LogInformation("User logged in successfully via biometric: {PhoneNumber}", request.PhoneNumber);
        return Ok(response);
    }

    // TODO: Implement 2FA endpoints (Phase 2)
    // POST /api/auth/2fa/init - Initiate 2FA challenge
    // POST /api/auth/2fa/validate - Validate OTP code
}

/// <summary>
/// Error response model
/// </summary>
public class ErrorResponse
{
    public required string Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Success response model
/// </summary>
public class SuccessResponse
{
    public required string Message { get; set; }
}
