namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a member dispute
/// </summary>
public enum DisputeStatus
{
    /// <summary>
    /// Dispute raised, being reviewed by Chairperson
    /// </summary>
    Open,
    
    /// <summary>
    /// Under discussion between parties
    /// </summary>
    InReview,
    
    /// <summary>
    /// Escalated to bank mediation (unresolved after 7 days)
    /// </summary>
    Escalated,
    
    /// <summary>
    /// Dispute resolved internally
    /// </summary>
    Resolved,
    
    /// <summary>
    /// Dispute closed without resolution
    /// </summary>
    Closed
}
