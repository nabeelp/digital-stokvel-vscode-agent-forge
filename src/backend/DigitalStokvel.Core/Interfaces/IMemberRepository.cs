using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Member-specific operations
/// </summary>
public interface IMemberRepository : IRepository<Member>
{
    Task<IEnumerable<Member>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetActiveByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Member?> GetByGroupIdAndUserIdAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Member>> GetByRoleAsync(Guid groupId, MemberRole role, CancellationToken cancellationToken = default);
    Task<Member?> GetChairpersonAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Member?> GetTreasurerAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<int> GetGroupMemberCountAsync(Guid groupId, CancellationToken cancellationToken = default);
}
