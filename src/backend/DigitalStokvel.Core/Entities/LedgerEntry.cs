using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Entities;

/// <summary>
/// Immutable ledger entry for all group transactions (GW-06)
/// No UPDATE or DELETE operations allowed
/// </summary>
public class LedgerEntry : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid? MemberId { get; set; } // Nullable for interest transactions
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public Member? Member { get; set; }
}
