namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Group constitution defining governance rules (GG-01)
/// One-to-one relationship with Group
/// </summary>
public class GroupConstitution : BaseEntity
{
    public Guid GroupId { get; set; }
    public string MissedPaymentPolicy { get; set; } = string.Empty;
    public decimal LateFeeAmount { get; set; }
    public decimal QuorumThreshold { get; set; } // Percentage for voting (e.g., 0.5 = 50%)
    public string MemberRemovalRules { get; set; } = string.Empty;
    public int GracePeriodDays { get; set; } = 3; // Days before payment marked overdue
    public bool AllowPartialPayments { get; set; } = false;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
}
