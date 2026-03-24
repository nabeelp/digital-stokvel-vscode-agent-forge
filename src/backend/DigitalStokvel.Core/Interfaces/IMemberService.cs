using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for managing member profiles and invitations
/// </summary>
public interface IMemberService
{
    Task<Result<MemberResponse>> GetMemberProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<MemberResponse>> GetMemberByIdAsync(Guid memberId, string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateMemberProfileAsync(UpdateMemberProfileRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> AcceptInvitationAsync(AcceptInvitationRequest request, string userId, CancellationToken cancellationToken = default);
}
