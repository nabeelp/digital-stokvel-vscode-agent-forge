using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Member entity representing a stokvel group member
/// </summary>
public class Member : BaseEntity
{
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty; // Bank customer ID
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty; // FICA requirement
    public MemberRole Role { get; set; } = MemberRole.Member;
    public MemberStatus Status { get; set; } = MemberStatus.Invited;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? InvitedAt { get; set; }
    public string BankAccountNumber { get; set; } = string.Empty;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<VoteRecord> VoteRecords { get; set; } = new List<VoteRecord>();
    public NotificationPreference? NotificationPreference { get; set; }
}
