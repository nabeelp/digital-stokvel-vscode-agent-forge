using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for governance and disputes (GG-01 to GG-09)
/// </summary>
public interface IGovernanceService
{
    Task<Result<ConstitutionResponse>> CreateConstitutionAsync(CreateConstitutionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<ConstitutionResponse>> GetConstitutionAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Result<VoteResponse>> CreateVoteAsync(CreateVoteRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<VoteResponse>> CastVoteAsync(Guid voteId, CastVoteRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<VoteResponse>> GetVoteDetailsAsync(Guid voteId, string userId, CancellationToken cancellationToken = default);
    Task<Result<DisputeResponse>> RaiseDisputeAsync(RaiseDisputeRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<DisputeResponse>> GetDisputeDetailsAsync(Guid disputeId, string userId, CancellationToken cancellationToken = default);
    Task<Result> AddDisputeMessageAsync(Guid disputeId, AddDisputeMessageRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> ResolveDisputeAsync(Guid disputeId, ResolveDisputeRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> CheckAndEscalateDisputesAsync(CancellationToken cancellationToken = default);
}
