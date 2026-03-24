namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Roles within a stokvel group
/// </summary>
public enum MemberRole
{
    /// <summary>
    /// Group leader (Umseki) - manages group, initiates payouts
    /// </summary>
    Chairperson,
    
    /// <summary>
    /// Financial overseer - must approve payouts
    /// </summary>
    Treasurer,
    
    /// <summary>
    /// Record keeper - maintains meeting minutes
    /// </summary>
    Secretary,
    
    /// <summary>
    /// Standard member - can contribute and vote
    /// </summary>
    Member
}
