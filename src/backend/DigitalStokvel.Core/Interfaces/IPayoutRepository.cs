using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Repository interface for Payout-specific operations
/// </summary>
public interface IPayoutRepository : IRepository<Payout>
{
    Task<IEnumerable<Payout>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payout>> GetPendingPayoutsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payout>> GetExpiredApprovalsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payout>> GetByRecipientMemberIdAsync(Guid recipientMemberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payout>> GetByStatusAsync(PayoutStatus status, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalPaidoutByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
}
