using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Contribution-specific operations
/// </summary>
public class ContributionRepository : Repository<Contribution>, IContributionRepository
{
    public ContributionRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Contribution>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Member)
            .Where(c => c.GroupId == groupId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contribution>> GetByGroupIdWithDateRangeAsync(Guid groupId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(c => c.Member)
            .Where(c => c.GroupId == groupId);

        if (from.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= to.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contribution>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contribution>> GetByStatusAsync(ContributionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Member)
            .Include(c => c.Group)
            .Where(c => c.Status == status)
            .OrderBy(c => c.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contribution>> GetOverdueContributionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(c => c.Member)
            .Include(c => c.Group)
            .Where(c => c.DueDate < now && 
                       (c.Status == ContributionStatus.Pending || c.Status == ContributionStatus.Overdue))
            .OrderBy(c => c.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.GroupId == groupId && c.Status == ContributionStatus.Completed)
            .SumAsync(c => c.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.MemberId == memberId && c.Status == ContributionStatus.Completed)
            .SumAsync(c => c.Amount, cancellationToken);
    }

    public async Task<decimal> GetTotalContributedAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.GroupId == groupId && c.Status == ContributionStatus.Completed)
            .SumAsync(c => c.Amount, cancellationToken);
    }

    public async Task<Contribution?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Member)
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<decimal> GetTotalByGroupAsync(Guid groupId, ContributionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.GroupId == groupId && c.Status == status)
            .SumAsync(c => c.Amount, cancellationToken);
    }
}
