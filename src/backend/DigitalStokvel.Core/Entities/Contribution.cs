using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Contribution entity representing a member's payment to the group
/// </summary>
public class Contribution : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid MemberId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public ContributionStatus Status { get; set; } = ContributionStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? FailureReason { get; set; }
    public int RetryCount { get; set; } = 0;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
