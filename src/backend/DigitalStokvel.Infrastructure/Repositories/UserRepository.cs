using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DigitalStokvelDbContext _context;

    public UserRepository(DigitalStokvelDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId && !u.IsDeleted);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
    }

    public async Task<User?> GetByIdNumberAsync(string idNumber)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.IdNumber == idNumber && !u.IsDeleted);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> SoftDeleteAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
    {
        return await _context.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber && !u.IsDeleted);
    }

    public async Task<bool> IdNumberExistsAsync(string idNumber)
    {
        return await _context.Users
            .AnyAsync(u => u.IdNumber == idNumber && !u.IsDeleted);
    }

    public async Task IncrementFailedLoginAttemptsAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts++;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ResetFailedLoginAttemptsAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task LockAccountAsync(string userId, DateTime unlockTime)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Locked;
        user.LockedUntil = unlockTime;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UnlockAccountAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        user.Status = UserStatus.Active;
        user.LockedUntil = null;
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLastLoginAsync(string userId, string ipAddress, string? deviceId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return;

        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        user.LastLoginDeviceId = deviceId;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
