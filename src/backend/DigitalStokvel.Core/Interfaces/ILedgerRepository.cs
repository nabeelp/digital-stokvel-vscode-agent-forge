using DigitalStokvel.Core.Entities;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for LedgerEntry-specific operations
/// IMPORTANT: No Update or Delete methods - ledger is immutable (GW-06)
/// </summary>
public interface ILedgerRepository
{
    Task<LedgerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerEntry>> GetByGroupIdAsync(Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerEntry>> GetByGroupIdWithDateRangeAsync(Guid groupId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<LedgerEntry>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<LedgerEntry> AddEntryAsync(LedgerEntry entry, CancellationToken cancellationToken = default);
    Task<int> CountByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
}
