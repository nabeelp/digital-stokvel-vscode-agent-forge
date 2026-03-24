using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using DigitalStokvel.Infrastructure.Repositories;

namespace DigitalStokvel.Infrastructure;

/// <summary>
/// Unit of Work implementation for transactional consistency (ACID compliance per NF-06)
/// Manages repository instances and coordinated SaveChanges with audit logging
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DigitalStokvelDbContext _context;
    private IGroupRepository? _groups;
    private IMemberRepository? _members;
    private IContributionRepository? _contributions;
    private IPayoutRepository? _payouts;
    private ILedgerRepository? _ledger;
    private IVoteRepository? _votes;
    private IDisputeRepository? _disputes;

    public UnitOfWork(DigitalStokvelDbContext context)
    {
        _context = context;
    }

    public IGroupRepository Groups => _groups ??= new GroupRepository(_context);

    public IMemberRepository Members => _members ??= new MemberRepository(_context);

    public IContributionRepository Contributions => _contributions ??= new ContributionRepository(_context);

    public IPayoutRepository Payouts => _payouts ??= new PayoutRepository(_context);

    public ILedgerRepository Ledger => _ledger ??= new LedgerRepository(_context);

    public IVoteRepository Votes => _votes ??= new VoteRepository(_context);

    public IDisputeRepository Disputes => _disputes ??= new DisputeRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement audit logging here (NF-07)
        // Capture before/after state for all modified entities
        // Write to AuditLogs table
        
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
