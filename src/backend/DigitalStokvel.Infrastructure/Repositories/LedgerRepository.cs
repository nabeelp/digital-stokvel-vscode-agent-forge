using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LedgerEntry-specific operations
/// IMPORTANT: No Update or Delete methods - ledger is immutable (GW-06)
/// </summary>
public class LedgerRepository : ILedgerRepository
{
    private readonly DigitalStokvelDbContext _context;
    private readonly DbSet<LedgerEntry> _dbSet;

    public LedgerRepository(DigitalStokvelDbContext context)
    {
        _context = context;
        _dbSet = context.Set<LedgerEntry>();
    }

    public async Task<LedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByGroupIdAsync(Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Member)
            .Where(l => l.GroupId == groupId)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByGroupIdWithDateRangeAsync(Guid groupId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(l => l.Member)
            .Where(l => l.GroupId == groupId && l.CreatedAt >= from && l.CreatedAt <= to)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LedgerEntry>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<LedgerEntry> AddEntryAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entry, cancellationToken);
        return entry;
    }

    public async Task<int> CountByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(l => l.GroupId == groupId, cancellationToken);
    }
}
