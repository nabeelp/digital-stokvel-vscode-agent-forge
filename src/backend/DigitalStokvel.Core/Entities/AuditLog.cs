namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Audit log for all state-changing operations (NF-07)
/// Retained for 7 years per regulatory requirements
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// User ID who performed the action (nullable for system actions)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Action type (Login, Logout, FailedLogin, PINChange, Create, Update, Delete, etc.)
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// Entity type affected (User, Group, Contribution, Payout, etc.)
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Entity ID affected (stored as string for flexibility)
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// State before the action (JSON serialized)
    /// </summary>
    public string? BeforeState { get; set; }

    /// <summary>
    /// State after the action (JSON serialized)
    /// </summary>
    public string? AfterState { get; set; }

    /// <summary>
    /// IP address of the request
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Device ID for tracking
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Success or failure flag
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Failure reason (for failed actions)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Additional metadata (JSON serialized)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Risk score (0-100) for security events
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// Timestamp of the action
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Audit log category for filtering
    /// </summary>
    public AuditCategory Category { get; set; } = AuditCategory.System;

    /// <summary>
    /// Severity level
    /// </summary>
    public AuditSeverity Severity { get; set; } = AuditSeverity.Information;

    // Navigation property
    /// <summary>
    /// User who performed the action
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// Audit log categories
/// </summary>
public enum AuditCategory
{
    Authentication = 1,
    Authorization = 2,
    DataAccess = 3,
    DataModification = 4,
    Security = 5,
    Compliance = 6,
    System = 7,
    FraudDetection = 8
}

/// <summary>
/// Audit log severity levels
/// </summary>
public enum AuditSeverity
{
    /// <summary>
    /// Informational event
    /// </summary>
    Information = 1,

    /// <summary>
    /// Warning event (e.g., failed login attempt)
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error event (e.g., multiple failed logins)
    /// </summary>
    Error = 3,

    /// <summary>
    /// Critical security event (e.g., account locked, suspicious activity)
    /// </summary>
    Critical = 4
}
