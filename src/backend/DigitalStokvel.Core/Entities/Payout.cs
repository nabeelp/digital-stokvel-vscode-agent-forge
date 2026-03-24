using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Payout entity representing a disbursement from the group wallet
/// </summary>
public class Payout : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid RecipientMemberId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestIncluded { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    public PayoutType PayoutType { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public Guid InitiatedByMemberId { get; set; }
    public Guid? ApprovedByMemberId { get; set; }
    public DateTime InitiatedAt { get; set; } // When payout was initiated
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ApprovalExpiresAt { get; set; } // 24-hour approval window
    public DateTime? ExpiresAt { get; set; } // 24-hour expiration per PE-03 (alias for ApprovalExpiresAt)
    public string? Notes { get; set; } // Optional notes from Chairperson
    public string? FailureReason { get; set; }
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public Member RecipientMember { get; set; } = null!;
}
