using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Interfaces;
using DigitalStokvel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalStokvel.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Group-specific operations
/// </summary>
public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(DigitalStokvelDbContext context) : base(context)
    {
    }

    public async Task<Group?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<Group?> GetByIdWithConstitutionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(g => g.Constitution)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Group>> GetByChairpersonIdAsync(Guid chairpersonId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(g => g.ChairpersonId == chairpersonId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Group>> GetActiveGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(g => g.Status == Core.Enums.GroupStatus.Active)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsBankAccountNumberUniqueAsync(string bankAccountNumber, Guid? excludeGroupId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(g => g.BankAccountNumber == bankAccountNumber);

        if (excludeGroupId.HasValue)
        {
            query = query.Where(g => g.Id != excludeGroupId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }
}
