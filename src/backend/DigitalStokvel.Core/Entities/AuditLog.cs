namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Audit log for all state-changing operations (NF-07)
/// Retained for 7 years per regulatory requirements
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // Group, Member, Contribution, Payout, etc.
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    public string? BeforeState { get; set; } // JSON snapshot
    public string? AfterState { get; set; } // JSON snapshot
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
