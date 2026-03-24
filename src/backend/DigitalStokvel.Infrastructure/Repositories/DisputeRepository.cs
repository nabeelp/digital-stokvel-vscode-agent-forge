using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Dispute-specific operations
/// </summary>
public class DisputeRepository : Repository<Dispute>, IDisputeRepository
{
    public DisputeRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Dispute>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.RaisedByMember)
            .Where(d => d.GroupId == groupId)
            .OrderByDescending(d => d.RaisedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Dispute>> GetByStatusAsync(DisputeStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Group)
            .Include(d => d.RaisedByMember)
            .Where(d => d.Status == status)
            .OrderBy(d => d.RaisedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dispute?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Messages)
            .Include(d => d.RaisedByMember)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Dispute>> GetDisputesPendingEscalationAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(d => d.Group)
            .Include(d => d.RaisedByMember)
            .Where(d => d.Status == DisputeStatus.Open && d.EscalationDeadline < now)
            .ToListAsync(cancellationToken);
    }
}
