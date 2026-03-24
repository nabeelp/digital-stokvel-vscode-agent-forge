namespace DigitalStokvel.Core.Enums;

/// <summary>
/// Status of a group member
/// </summary>
public enum MemberStatus
{
    /// <summary>
    /// Active member in good standing
    /// </summary>
    Active,
    
    /// <summary>
    /// Member has been removed or left
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Member removed from group
    /// </summary>
    Removed,
    
    /// <summary>
    /// Pending acceptance of group invitation
    /// </summary>
    Invited
}
