using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Vote-specific operations
/// </summary>
public class VoteRepository : Repository<Vote>, IVoteRepository
{
    public VoteRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Vote>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.GroupId == groupId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vote>> GetActiveVotesAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(v => v.GroupId == groupId && 
                       v.Status == VoteStatus.Active && 
                       v.VoteDeadline > now)
            .OrderBy(v => v.VoteDeadline)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vote?> GetByIdWithRecordsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(v => v.VoteRecords)
                .ThenInclude(vr => vr.Member)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Vote>> GetExpiredVotesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(v => v.Status == VoteStatus.Active && v.VoteDeadline < now)
            .ToListAsync(cancellationToken);
    }
}
