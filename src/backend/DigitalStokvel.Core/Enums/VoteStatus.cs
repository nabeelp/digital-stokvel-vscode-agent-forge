namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a governance vote
/// </summary>
public enum VoteStatus
{
    /// <summary>
    /// Vote is active and accepting responses
    /// </summary>
    Active,
    
    /// <summary>
    /// Vote passed with required majority
    /// </summary>
    Passed,
    
    /// <summary>
    /// Vote did not achieve required majority
    /// </summary>
    Failed,
    
    /// <summary>
    /// Vote cancelled by Chairperson
    /// </summary>
    Cancelled
}
