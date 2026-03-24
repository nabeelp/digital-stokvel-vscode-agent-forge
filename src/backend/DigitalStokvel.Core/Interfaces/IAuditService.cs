namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Audit service interface for compliance (NF-07, SP-12)
/// </summary>
public interface IAuditService
{
    Task LogAuthenticationEventAsync(string? userId, string action, string? ipAddress, string? userAgent, bool success, string? failureReason = null);
    Task LogDataAccessAsync(string userId, string entityType, string entityId, string accessType);
    Task LogSecurityEventAsync(string? userId, string eventType, string details, int riskScore = 0);
    Task LogDataModificationAsync(string userId, string entityType, string entityId, string action, object? beforeState, object? afterState);
}
