using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AuditLog entity
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly DigitalStokvelDbContext _context;

    public AuditLogRepository(DigitalStokvelDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId, int limit = 100)
    {
        return await _context.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId, int limit = 100)
    {
        return await _context.AuditLogs
            .Where(al => al.EntityType == entityType && al.EntityId == entityId)
            .OrderByDescending(al => al.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByCategoryAsync(AuditCategory category, DateTime? fromDate, DateTime? toDate, int limit = 100)
    {
        var query = _context.AuditLogs.Where(al => al.Category == category);

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(al => al.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action, DateTime? fromDate, DateTime? toDate, int limit = 100)
    {
        var query = _context.AuditLogs.Where(al => al.Action == action);

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(al => al.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetSecurityEventsAsync(DateTime? fromDate, DateTime? toDate, int limit = 100)
    {
        var query = _context.AuditLogs
            .Where(al => al.Category == AuditCategory.Security || 
                        al.Category == AuditCategory.FraudDetection ||
                        al.Severity >= AuditSeverity.Error);

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(al => al.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetFailedAuthenticationAttemptsAsync(string userId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.AuditLogs
            .Where(al => al.UserId == userId && 
                        al.Category == AuditCategory.Authentication && 
                        !al.Success);

        if (fromDate.HasValue)
            query = query.Where(al => al.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.Timestamp <= toDate.Value);

        return await query
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync();
    }

    public async Task<int> DeleteOldLogsAsync(DateTime olderThan)
    {
        var oldLogs = await _context.AuditLogs
            .Where(al => al.Timestamp < olderThan)
            .ToListAsync();

        _context.AuditLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();
        return oldLogs.Count;
    }

    public async Task<Dictionary<string, int>> GetStatisticsAsync(DateTime fromDate, DateTime toDate)
    {
        var stats = new Dictionary<string, int>();

        var logs = await _context.AuditLogs
            .Where(al => al.Timestamp >= fromDate && al.Timestamp <= toDate)
            .ToListAsync();

        stats["Total"] = logs.Count;
        stats["Success"] = logs.Count(al => al.Success);
        stats["Failed"] = logs.Count(al => !al.Success);
        stats["Authentication"] = logs.Count(al => al.Category == AuditCategory.Authentication);
        stats["Security"] = logs.Count(al => al.Category == AuditCategory.Security);
        stats["FraudDetection"] = logs.Count(al => al.Category == AuditCategory.FraudDetection);
        stats["Critical"] = logs.Count(al => al.Severity == AuditSeverity.Critical);

        return stats;
    }
}
