using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for managing contributions (CC-01 to CC-10)
/// </summary>
public interface IContributionService
{
    Task<Result<ContributionResponse>> ProcessContributionAsync(CreateContributionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<ContributionReceiptResponse>> GetContributionReceiptAsync(Guid contributionId, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<ContributionResponse>>> GetMyContributionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> SetupRecurringContributionAsync(SetupRecurringContributionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> CancelRecurringContributionAsync(Guid recurringId, string userId, CancellationToken cancellationToken = default);
    Task<Result<ContributionResponse>> RetryFailedContributionAsync(Guid contributionId, string userId, CancellationToken cancellationToken = default);
    Task<Result<MemberContributionHistory>> GetMemberContributionHistoryAsync(Guid groupId, Guid memberId, CancellationToken cancellationToken = default);
}
