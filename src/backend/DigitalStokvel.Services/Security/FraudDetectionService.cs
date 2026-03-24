using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Security;

/// <summary>
/// Fraud detection service for identifying suspicious activities (SP-15)
/// </summary>
public class FraudDetectionService : IFraudDetectionService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FraudDetectionService> _logger;
    private readonly INotificationService _notificationService;

    public FraudDetectionService(
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository,
        ILogger<FraudDetectionService> logger,
        INotificationService notificationService)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<FraudCheckResult> DetectSuspiciousLoginAsync(string userId, string ipAddress, string deviceId)
    {
        var result = new FraudCheckResult();
        var riskFactors = new List<string>();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return result;
        }

        // Check for new device
        if (!string.IsNullOrEmpty(user.LastLoginDeviceId) && user.LastLoginDeviceId != deviceId)
        {
            result.RiskScore += 30;
            riskFactors.Add("New device detected");
        }

        // Check for different IP address
        if (!string.IsNullOrEmpty(user.LastLoginIp) && user.LastLoginIp != ipAddress)
        {
            result.RiskScore += 20;
            riskFactors.Add("Different IP address");
        }

        // Check for unusual location (IP outside South Africa)
        if (!string.IsNullOrEmpty(ipAddress) && !IsIPAddressInSouthAfrica(ipAddress))
        {
            result.RiskScore += 40;
            riskFactors.Add("IP address outside South Africa");
        }

        // Check for multiple recent failed login attempts
        var recentFailedAttempts = await _auditLogRepository.GetFailedAuthenticationAttemptsAsync(
            userId, 
            DateTime.UtcNow.AddHours(-1), 
            DateTime.UtcNow
        );
        
        if (recentFailedAttempts.Count() >= 2)
        {
            result.RiskScore += 25;
            riskFactors.Add($"{recentFailedAttempts.Count()} failed login attempts in last hour");
        }

        // Check for rapid succession logins (velocity check)
        if (user.LastLoginAt.HasValue)
        {
            var minutesSinceLastLogin = (DateTime.UtcNow - user.LastLoginAt.Value).TotalMinutes;
            if (minutesSinceLastLogin < 2)
            {
                result.RiskScore += 15;
                riskFactors.Add("Rapid succession login");
            }
        }

        result.IsSuspicious = result.RiskScore >= 50;
        result.RiskFactors = riskFactors;

        if (result.IsSuspicious)
        {
            _logger.LogWarning("Suspicious login detected for user {UserId}. Risk score: {RiskScore}. Factors: {Factors}", 
                userId, result.RiskScore, string.Join(", ", riskFactors));

            // Log fraud detection event
            await LogFraudEventAsync(userId, "SuspiciousLogin", result.RiskScore, riskFactors);

            // Send alert to user
            if (result.RiskScore >= 70)
            {
                await _notificationService.SendSmsAsync(
                    user.PhoneNumber,
                    $"Suspicious login detected on your Digital Stokvel account. If this wasn't you, please contact support immediately."
                );
            }
        }

        return result;
    }

    public async Task<FraudCheckResult> DetectSuspiciousContributionAsync(string contributionId)
    {
        var result = new FraudCheckResult();
        var riskFactors = new List<string>();

        // TODO: Implement contribution fraud detection
        // - Check for unusual amounts
        // - Check for frequency of contributions
        // - Check for contributions from new accounts

        result.IsSuspicious = result.RiskScore >= 50;
        result.RiskFactors = riskFactors;

        return result;
    }

    public async Task<FraudCheckResult> DetectSuspiciousPayoutAsync(string payoutId)
    {
        var result = new FraudCheckResult();
        var riskFactors = new List<string>();

        // TODO: Implement payout fraud detection
        // - Check for large amounts (>R5,000 requires 2FA per SP-02)
        // - Check for unusual payout patterns
        // - Check for rapid succession of payouts

        result.IsSuspicious = result.RiskScore >= 50;
        result.RiskFactors = riskFactors;

        return result;
    }

    public async Task FlagForReviewAsync(string entityId, string entityType, string reason)
    {
        _logger.LogWarning("Entity flagged for review: {EntityType} {EntityId}. Reason: {Reason}", 
            entityType, entityId, reason);

        // Log fraud detection event
        await LogFraudEventAsync(null, "FlaggedForReview", 75, new List<string> { reason });

        // TODO: Notify compliance team
    }

    private bool IsIPAddressInSouthAfrica(string ipAddress)
    {
        // TODO: Implement proper IP geolocation check
        // For MVP, we'll simulate this check
        // In production, use a geolocation service or IP database
        return true;
    }

    private async Task LogFraudEventAsync(string? userId, string action, int riskScore, List<string> riskFactors)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = "FraudDetection",
            Success = true,
            RiskScore = riskScore,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { RiskFactors = riskFactors }),
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.FraudDetection,
            Severity = riskScore >= 70 ? AuditSeverity.Critical : AuditSeverity.Warning
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }
}
