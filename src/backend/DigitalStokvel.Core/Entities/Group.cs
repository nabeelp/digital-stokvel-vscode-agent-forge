using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Stokvel Group entity representing a savings group
/// </summary>
public class Group : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GroupType GroupType { get; set; }
    public decimal ContributionAmount { get; set; }
    public ContributionFrequency ContributionFrequency { get; set; }
    public PayoutSchedule PayoutSchedule { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalInterestEarned { get; set; }
    public GroupStatus Status { get; set; } = GroupStatus.Active;
    public string BankAccountNumber { get; set; } = string.Empty;
    public Guid ChairpersonId { get; set; }
    public Guid? TreasurerId { get; set; }
    
    // Navigation properties
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
    public ICollection<Payout> Payouts { get; set; } = new List<Payout>();
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    public ICollection<InterestTransaction> InterestTransactions { get; set; } = new List<InterestTransaction>();
    public GroupConstitution? Constitution { get; set; }
}
