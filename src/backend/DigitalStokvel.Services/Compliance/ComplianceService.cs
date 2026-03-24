using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DigitalStokvel.Services.Compliance;

/// <summary>
/// POPIA compliance service for data privacy and protection (SP-07, SP-13, SP-14)
/// </summary>
public class ComplianceService : IComplianceService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<ComplianceService> _logger;

    public ComplianceService(
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        ILogger<ComplianceService> logger)
    {
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public string MaskPhoneNumber(string phoneNumber)
    {
        // Mask phone number: +27821234567 -> +2782****567 (SP-06, SP-13)
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 10)
        {
            return phoneNumber;
        }

        var prefix = phoneNumber[..6]; // +27821
        var suffix = phoneNumber[^3..]; // 567
        return $"{prefix}****{suffix}";
    }

    public string MaskIdNumber(string idNumber)
    {
        // Mask ID number: 8501015800082 -> 8501****0**2 (SP-06, SP-13)
        if (string.IsNullOrEmpty(idNumber) || idNumber.Length != 13)
        {
            return idNumber;
        }

        var prefix = idNumber[..4]; // 8501
        var suffix = idNumber[^1]; // 2
        return $"{prefix}****0**{suffix}";
    }

    public async Task<byte[]> ExportUserDataAsync(string userId)
    {
        // Export all user data for POPIA data portability rights (SP-07, SP-14)
        _logger.LogInformation("Exporting user data for user {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Get audit logs
        var auditLogs = await _auditLogRepository.GetByUserIdAsync(userId, limit: 1000);

        // Create export package
        var exportData = new StringBuilder();
        exportData.AppendLine("=== Digital Stokvel Banking - User Data Export ===");
        exportData.AppendLine($"Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        exportData.AppendLine($"User ID: {user.Id}");
        exportData.AppendLine();
        
        exportData.AppendLine("=== Personal Information ===");
        exportData.AppendLine($"Full Name: {user.FullName}");
        exportData.AppendLine($"Phone Number: {user.PhoneNumber}");
        exportData.AppendLine($"ID Number: {MaskIdNumber(user.IdNumber)}");
        exportData.AppendLine($"Email: {user.Email ?? "Not provided"}");
        exportData.AppendLine($"Account Status: {user.Status}");
        exportData.AppendLine($"FICA Verified: {user.FICAVerified}");
        exportData.AppendLine($"Created At: {user.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        exportData.AppendLine($"Last Login At: {user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"} UTC");
        exportData.AppendLine();

        exportData.AppendLine("=== Security Settings ===");
        exportData.AppendLine($"Biometric Enabled: {user.BiometricEnabled}");
        exportData.AppendLine($"Two-Factor Enabled: {user.TwoFactorEnabled}");
        exportData.AppendLine();

        exportData.AppendLine("=== Consent Records ===");
        exportData.AppendLine($"POPIA Consent: {(user.POPIAConsentAccepted ? "Accepted" : "Not accepted")}");
        exportData.AppendLine($"POPIA Consent Date: {user.POPIAConsentAcceptedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"} UTC");
        exportData.AppendLine();

        exportData.AppendLine("=== Activity Log (Last 1000 entries) ===");
        foreach (var log in auditLogs)
        {
            exportData.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.Action} - {(log.Success ? "Success" : "Failed")}");
        }

        // Log data export event
        await LogDataAccessEventAsync(userId, "DataExport");

        return Encoding.UTF8.GetBytes(exportData.ToString());
    }

    public async Task<bool> DeleteUserDataAsync(string userId)
    {
        // Soft delete user data for POPIA erasure rights (SP-07, SP-14)
        _logger.LogInformation("Deleting user data for user {UserId}", userId);

        var deleted = await _userRepository.SoftDeleteAsync(userId);
        
        if (deleted)
        {
            // Log data deletion event
            await LogDataAccessEventAsync(userId, "DataDeletion");
        }

        return deleted;
    }

    public async Task AuditDataAccessAsync(string userId, string accessedBy, string purpose)
    {
        // Log PII access for POPIA compliance (SP-12)
        _logger.LogInformation("PII accessed for user {UserId} by {AccessedBy} for purpose: {Purpose}", 
            userId, accessedBy, purpose);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = accessedBy,
            Action = "DataAccess",
            EntityType = "User",
            EntityId = userId,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { Purpose = purpose }),
            Success = true,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.DataAccess,
            Severity = AuditSeverity.Information
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public bool ShouldMaskPii(string requestingUserId, string targetUserId, string userRole)
    {
        // Determine if PII should be masked based on role and relationship (SP-06, SP-13)
        // Full PII visible only to:
        // 1. The user themselves
        // 2. Chairperson viewing group members
        // 3. Administrators/compliance team

        if (requestingUserId == targetUserId)
        {
            return false; // User can see their own data
        }

        if (userRole == "Chairperson" || userRole == "Administrator" || userRole == "Compliance")
        {
            return false; // Chairperson and admins can see full data
        }

        return true; // Mask for everyone else
    }

    private async Task LogDataAccessEventAsync(string userId, string action)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = "User",
            EntityId = userId,
            Success = true,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.Compliance,
            Severity = AuditSeverity.Information
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }
}
