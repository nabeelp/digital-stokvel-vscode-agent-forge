namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Unit of Work pattern for transactional consistency (ACID compliance per NF-06)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGroupRepository Groups { get; }
    IMemberRepository Members { get; }
    IContributionRepository Contributions { get; }
    IPayoutRepository Payouts { get; }
    ILedgerRepository Ledger { get; }
    IVoteRepository Votes { get; }
    IDisputeRepository Disputes { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
