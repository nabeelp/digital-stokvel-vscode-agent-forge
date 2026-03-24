namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Type of ledger transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Member contribution (credit to group wallet)
    /// </summary>
    Contribution,
    
    /// <summary>
    /// Payout to member (debit from group wallet)
    /// </summary>
    Payout,
    
    /// <summary>
    /// Monthly interest capitalization (credit to group wallet)
    /// </summary>
    Interest,
    
    /// <summary>
    /// Monthly interest capitalization (credit to group wallet)
    /// </summary>
    InterestCapitalization,
    
    /// <summary>
    /// Late fee applied to member (credit to group wallet)
    /// </summary>
    LateFee,
    
    /// <summary>
    /// Refund to member (debit from group wallet)
    /// </summary>
    Refund
}
