using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Dispute entity for conflict resolution (GG-06)
/// </summary>
public class Dispute : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid RaisedByMemberId { get; set; }
    public string IssueType { get; set; } = string.Empty; // MissedPayment, IncorrectAmount, UnauthorizedWithdrawal
    public string Description { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; } = DisputeStatus.Open;
    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public DateTime EscalationDeadline { get; set; } // 7 days from creation
    public string? Resolution { get; set; }
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public Member RaisedByMember { get; set; } = null!;
    public ICollection<DisputeMessage> Messages { get; set; } = new List<DisputeMessage>();
    public ICollection<DisputeMessage> DisputeMessages { get; set; } = new List<DisputeMessage>(); // Alias for Messages
}
