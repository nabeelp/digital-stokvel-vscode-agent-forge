namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a stokvel group
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Group is active and accepting contributions
    /// </summary>
    Active,
    
    /// <summary>
    /// Group is temporarily paused (no contributions)
    /// </summary>
    Paused,
    
    /// <summary>
    /// Group is archived (no new contributions, funds accessible)
    /// </summary>
    Archived
}
