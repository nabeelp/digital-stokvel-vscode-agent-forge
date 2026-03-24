using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Security;

/// <summary>
/// Audit logging service for compliance and security tracking (NF-07, SP-12)
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogAuthenticationEventAsync(string? userId, string action, string? ipAddress, string? userAgent, bool success, string? failureReason = null)
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

        _logger.LogInformation("Authentication event logged: {Action} for user {UserId}. Success: {Success}", 
            action, userId ?? "unknown", success);
    }

    public async Task LogDataAccessAsync(string userId, string entityType, string entityId, string accessType)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = accessType,
            EntityType = entityType,
            EntityId = entityId,
            Success = true,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.DataAccess,
            Severity = AuditSeverity.Information
        };

        await _auditLogRepository.CreateAsync(auditLog);
    }

    public async Task LogSecurityEventAsync(string? userId, string eventType, string details, int riskScore = 0)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = eventType,
            EntityType = "Security",
            Success = true,
            RiskScore = riskScore,
            Metadata = details,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.Security,
            Severity = riskScore >= 70 ? AuditSeverity.Critical : 
                      riskScore >= 50 ? AuditSeverity.Error : 
                      AuditSeverity.Warning
        };

        await _auditLogRepository.CreateAsync(auditLog);

        _logger.LogWarning("Security event logged: {EventType} for user {UserId}. Risk score: {RiskScore}", 
            eventType, userId ?? "unknown", riskScore);
    }

    public async Task LogDataModificationAsync(string userId, string entityType, string entityId, string action, object? beforeState, object? afterState)
    {
        var beforeJson = beforeState != null ? System.Text.Json.JsonSerializer.Serialize(beforeState) : null;
        var afterJson = afterState != null ? System.Text.Json.JsonSerializer.Serialize(afterState) : null;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeState = beforeJson,
            AfterState = afterJson,
            Success = true,
            Timestamp = DateTime.UtcNow,
            Category = AuditCategory.DataModification,
            Severity = AuditSeverity.Information
        };

        await _auditLogRepository.CreateAsync(auditLog);

        _logger.LogInformation("Data modification logged: {Action} on {EntityType} {EntityId} by user {UserId}", 
            action, entityType, entityId, userId);
    }
}
