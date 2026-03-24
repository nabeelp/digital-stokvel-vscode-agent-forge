using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Contribution-specific operations
/// </summary>
public interface IContributionRepository : IRepository<Contribution>
{
    Task<IEnumerable<Contribution>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contribution>> GetByGroupIdWithDateRangeAsync(Guid groupId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contribution>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contribution>> GetByStatusAsync(ContributionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contribution>> GetOverdueContributionsAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalContributedAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByGroupAsync(Guid groupId, ContributionStatus status, CancellationToken cancellationToken = default);
    Task<Contribution?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
