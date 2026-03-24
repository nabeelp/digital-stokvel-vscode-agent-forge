using DigitalStokvel.Core.Entities;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for AuditLog entity
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Create audit log entry
    /// </summary>
    Task<AuditLog> CreateAsync(AuditLog auditLog);

    /// <summary>
    /// Get audit logs by user ID
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100);

    /// <summary>
    /// Get audit logs by entity
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId, int limit = 100);

    /// <summary>
    /// Get audit logs by category
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByCategoryAsync(AuditCategory category, DateTime? fromDate, DateTime? toDate, int limit = 100);

    /// <summary>
    /// Get audit logs by action
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate, DateTime? toDate, int limit = 100);

    /// <summary>
    /// Get security events (high severity)
    /// </summary>
    Task<IEnumerable<AuditLog>> GetSecurityEventsAsync(DateTime? fromDate, DateTime? toDate, int limit = 100);

    /// <summary>
    /// Get failed authentication attempts for a user
    /// </summary>
    Task<IEnumerable<AuditLog>> GetFailedAuthenticationAttemptsAsync(string userId, DateTime? fromDate, DateTime? toDate);

    /// <summary>
    /// Delete old audit logs (7-year retention policy)
    /// </summary>
    Task<int> DeleteOldLogsAsync(DateTime olderThan);

    /// <summary>
    /// Get audit log statistics
    /// </summary>
    Task<Dictionary<string, int>> GetStatisticsAsync(DateTime fromDate, DateTime toDate);
}
