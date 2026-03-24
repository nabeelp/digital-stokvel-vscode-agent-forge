using DigitalStokvel.Core.Entities;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(string userId);

    /// <summary>
    /// Get user by phone number
    /// </summary>
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);

    /// <summary>
    /// Get user by ID number
    /// </summary>
    Task<User?> GetByIdNumberAsync(string idNumber);

    /// <summary>
    /// Create new user
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Update existing user
    /// </summary>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Soft delete user (POPIA compliance)
    /// </summary>
    Task<bool> SoftDeleteAsync(string userId);

    /// <summary>
    /// Check if phone number already exists
    /// </summary>
    Task<bool> PhoneNumberExistsAsync(string phoneNumber);

    /// <summary>
    /// Check if ID number already exists
    /// </summary>
    Task<bool> IdNumberExistsAsync(string idNumber);

    /// <summary>
    /// Increment failed login attempts
    /// </summary>
    Task IncrementFailedLoginAttemptsAsync(string userId);

    /// <summary>
    /// Reset failed login attempts
    /// </summary>
    Task ResetFailedLoginAttemptsAsync(string userId);

    /// <summary>
    /// Lock user account
    /// </summary>
    Task LockAccountAsync(string userId, DateTime unlockTime);

    /// <summary>
    /// Unlock user account
    /// </summary>
    Task UnlockAccountAsync(string userId);

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(string userId, string ipAddress, string? deviceId);
}
