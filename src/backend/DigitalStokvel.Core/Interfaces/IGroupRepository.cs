using DigitalStokvel.Core.Entities;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Group-specific operations
/// </summary>
public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Group?> GetByIdWithConstitutionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Group>> GetByChairpersonIdAsync(Guid chairpersonId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Group>> GetActiveGroupsAsync(CancellationToken cancellationToken = default);
    Task<bool> IsBankAccountNumberUniqueAsync(string bankAccountNumber, Guid? excludeGroupId = null, CancellationToken cancellationToken = default);
}
