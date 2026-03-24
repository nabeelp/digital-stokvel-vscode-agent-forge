using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Vote entity for group governance decisions (GG-02)
/// </summary>
public class Vote : BaseEntity
{
    public Guid GroupId { get; set; }
    public string Proposal { get; set; } = string.Empty;
    public Guid CreatedByMemberId { get; set; }
    public DateTime VoteDeadline { get; set; }
    public decimal QuorumThreshold { get; set; } // Percentage required (e.g., 0.5 = 50%)
    public VoteStatus Status { get; set; } = VoteStatus.Active;
    public DateTime? CompletedAt { get; set; }
    public int YesCount { get; set; } = 0;
    public int NoCount { get; set; } = 0;
    public int AbstainCount { get; set; } = 0;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public ICollection<VoteRecord> VoteRecords { get; set; } = new List<VoteRecord>();
}
