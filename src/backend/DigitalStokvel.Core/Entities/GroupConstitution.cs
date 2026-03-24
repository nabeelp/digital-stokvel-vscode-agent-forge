namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Group constitution defining governance rules (GG-01)
/// One-to-one relationship with Group
/// </summary>
public class GroupConstitution : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid? CreatedByMemberId { get; set; } // Member who created/last updated the constitution
    public string MissedPaymentPolicy { get; set; } = string.Empty;
    public decimal LateFeeAmount { get; set; }
    public decimal QuorumThreshold { get; set; } // Percentage for voting (e.g., 0.5 = 50%)
    public int QuorumPercentage { get; set; } = 50; // Voting quorum threshold
    public string MemberRemovalRules { get; set; } = string.Empty;
    public string MemberRemovalProcess { get; set; } = string.Empty; // How members are removed
    public string OtherRules { get; set; } = string.Empty; // Additional custom rules
    public int GracePeriodDays { get; set; } = 3; // Days before payment marked overdue
    public bool AllowPartialPayments { get; set; } = false;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
}
