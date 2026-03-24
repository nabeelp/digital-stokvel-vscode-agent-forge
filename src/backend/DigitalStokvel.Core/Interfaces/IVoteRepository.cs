using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Vote-specific operations
/// </summary>
public interface IVoteRepository : IRepository<Vote>
{
    Task<IEnumerable<Vote>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vote>> GetActiveVotesAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Vote?> GetByIdWithRecordsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Vote>> GetExpiredVotesAsync(CancellationToken cancellationToken = default);
}
