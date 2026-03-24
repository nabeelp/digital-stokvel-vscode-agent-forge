namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a contribution payment
/// </summary>
public enum ContributionStatus
{
    /// <summary>
    /// Payment is pending processing
    /// </summary>
    Pending,
    
    /// <summary>
    /// Payment is being processed
    /// </summary>
    Processing,
    
    /// <summary>
    /// Payment confirmed and logged on ledger
    /// </summary>
    Completed,
    
    /// <summary>
    /// Payment failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Payment is overdue (3+ days past due date)
    /// </summary>
    Overdue,
    
    /// <summary>
    /// Payment is escalated (7+ days past due date)
    /// </summary>
    Escalated
}
