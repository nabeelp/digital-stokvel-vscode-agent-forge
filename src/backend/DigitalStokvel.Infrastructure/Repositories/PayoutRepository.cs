using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Payout-specific operations
/// </summary>
public class PayoutRepository : Repository<Payout>, IPayoutRepository
{
    public PayoutRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payout>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.RecipientMember)
            .Where(p => p.GroupId == groupId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payout>> GetPendingPayoutsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Group)
            .Include(p => p.RecipientMember)
            .Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.Approved)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payout>> GetExpiredApprovalsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(p => p.Group)
            .Where(p => p.Status == PayoutStatus.Pending && p.ApprovalExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payout>> GetByRecipientMemberIdAsync(Guid recipientMemberId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.RecipientMemberId == recipientMemberId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payout>> GetByStatusAsync(PayoutStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Group)
            .Include(p => p.RecipientMember)
            .Where(p => p.Status == status)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaidoutByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.GroupId == groupId && p.Status == PayoutStatus.Completed)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<Payout?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.RecipientMember)
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<decimal> GetTotalByGroupAsync(Guid groupId, PayoutStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.GroupId == groupId && p.Status == status)
            .SumAsync(p => p.Amount, cancellationToken);
    }
}
