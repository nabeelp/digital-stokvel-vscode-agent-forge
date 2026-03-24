using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Member-specific operations
/// </summary>
public class MemberRepository : Repository<Member>, IMemberRepository
{
    public MemberRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetActiveByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.GroupId == groupId && m.Status == MemberStatus.Active)
            .OrderBy(m => m.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Group)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Member?> GetByGroupIdAndUserIdAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Member>> GetByRoleAsync(Guid groupId, MemberRole role, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.GroupId == groupId && m.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<Member?> GetChairpersonAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.Role == MemberRole.Chairperson, cancellationToken);
    }

    public async Task<Member?> GetTreasurerAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.Role == MemberRole.Treasurer, cancellationToken);
    }
}
