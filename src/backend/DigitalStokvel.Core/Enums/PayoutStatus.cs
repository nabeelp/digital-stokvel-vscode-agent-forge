namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a payout transaction
/// </summary>
public enum PayoutStatus
{
    /// <summary>
    /// Payout initiated by Chairperson, awaiting Treasurer approval
    /// </summary>
    Pending,
    
    /// <summary>
    /// Payout initiated, awaiting Treasurer approval
    /// </summary>
    PendingApproval,
    
    /// <summary>
    /// Payout approved by Treasurer, awaiting execution
    /// </summary>
    Approved,
    
    /// <summary>
    /// EFT completed successfully
    /// </summary>
    Completed,
    
    /// <summary>
    /// Payout failed (e.g., invalid account)
    /// </summary>
    Failed,
    
    /// <summary>
    /// Payout rejected by Treasurer or expired (24h timeout)
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Not approved within 24 hours
    /// </summary>
    Expired
}
