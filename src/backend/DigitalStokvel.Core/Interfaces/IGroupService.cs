using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for managing stokvel groups (GM-01 to GM-09)
/// </summary>
public interface IGroupService
{
    Task<Result<GroupResponse>> CreateGroupAsync(CreateGroupRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<GroupDetailsResponse>> GetGroupDetailsAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<Result<List<GroupResponse>>> GetMyGroupsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateGroupAsync(Guid groupId, UpdateGroupRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> ArchiveGroupAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<Result> AddMemberAsync(Guid groupId, InviteMemberRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(Guid groupId, Guid memberId, RemoveMemberRequest request, string userId, CancellationToken cancellationToken = default);
    Task<Result<decimal>> CalculateGroupBalanceAsync(Guid groupId, CancellationToken cancellationToken = default);
}
