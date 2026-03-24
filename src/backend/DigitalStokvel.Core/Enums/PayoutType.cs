namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Type of payout distribution
/// </summary>
public enum PayoutType
{
    /// <summary>
    /// Single member receives full payout in rotation
    /// </summary>
    Rotating,
    
    /// <summary>
    /// All members receive proportional share
    /// </summary>
    Proportional,
    
    /// <summary>
    /// Emergency or special payout
    /// </summary>
    Emergency
}
