using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Individual member's vote on a proposal (GG-03)
/// </summary>
public class VoteRecord : BaseEntity
{
    public Guid VoteId { get; set; }
    public Guid MemberId { get; set; }
    public VoteChoice VoteChoice { get; set; }
    public VoteChoice Choice { get; set; } // Alias for VoteChoice for service compatibility
    public string? Comments { get; set; } // Optional comments from voter
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Vote Vote { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
