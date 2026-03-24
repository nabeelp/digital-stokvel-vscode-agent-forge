using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for managing payouts (PE-01 to PE-09)
/// </summary>
public interface IPayoutService
{
    Task<Result<PayoutResponse>> InitiatePayoutAsync(InitiatePayoutRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<PayoutResponse>> ApprovePayoutAsync(Guid payoutId, ApprovePayoutRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> RejectPayoutAsync(Guid payoutId, RejectPayoutRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<PayoutResponse>> GetPayoutDetailsAsync(Guid payoutId, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<PayoutHistoryItem>>> GetGroupPayoutHistoryAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<PayoutResponse>>> GetPendingApprovalsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<NextPayoutRecipientResponse>> CalculateNextPayoutRecipientAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Result> ExecutePayoutAsync(Guid payoutId, CancellationToken cancellationToken = default);
}
