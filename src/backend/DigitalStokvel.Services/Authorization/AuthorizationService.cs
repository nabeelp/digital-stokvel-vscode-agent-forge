using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Authorization;

/// <summary>
/// Authorization service for role-based access control (RBAC) (SP-03)
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IPayoutRepository _payoutRepository;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IMemberRepository memberRepository,
        IGroupRepository groupRepository,
        IPayoutRepository payoutRepository,
        ILogger<AuthorizationService> logger)
    {
        _memberRepository = memberRepository;
        _groupRepository = groupRepository;
        _payoutRepository = payoutRepository;
        _logger = logger;
    }

    public async Task<bool> CheckGroupMembershipAsync(string userId, Guid groupId)
    {
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        var isMember = member != null;
        
        if (!isMember)
        {
            _logger.LogWarning("Authorization failed: User {UserId} is not a member of group {GroupId}", userId, groupId);
        }

        return isMember;
    }

    public async Task<bool> CheckRoleAsync(string userId, Guid groupId, MemberRole requiredRole)
    {
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        
        if (member == null)
        {
            _logger.LogWarning("Authorization failed: User {UserId} not found in group {GroupId}", userId, groupId);
            return false;
        }

        var hasRole = member.Role == requiredRole;

        if (!hasRole)
        {
            _logger.LogWarning("Authorization failed: User {UserId} has role {ActualRole} but {RequiredRole} is required", 
                userId, member.Role, requiredRole);
        }

        return hasRole;
    }

    public async Task<bool> CheckPayoutApprovalPermissionAsync(string userId, Guid payoutId)
    {
        // Payout approval requires:
        // 1. Must be Treasurer (not Chairperson) per SP-03
        // 2. Chairperson initiates, Treasurer approves (dual approval)

        var payout = await _payoutRepository.GetByIdAsync(payoutId);
        if (payout == null)
        {
            _logger.LogWarning("Payout {PayoutId} not found", payoutId);
            return false;
        }

        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(payout.GroupId, userId);
        if (member == null)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}", userId, payout.GroupId);
            return false;
        }

        // Only Treasurer can approve payouts
        var canApprove = member.Role == MemberRole.Treasurer;

        if (!canApprove)
        {
            _logger.LogWarning("Authorization failed: User {UserId} with role {Role} cannot approve payouts. Treasurer role required.", 
                userId, member.Role);
        }

        return canApprove;
    }

    public async Task<bool> CheckPayoutInitiationPermissionAsync(string userId, Guid groupId)
    {
        // Only Chairperson can initiate payouts (SP-03)
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        if (member == null)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}", userId, groupId);
            return false;
        }

        var canInitiate = member.Role == MemberRole.Chairperson;

        if (!canInitiate)
        {
            _logger.LogWarning("Authorization failed: User {UserId} with role {Role} cannot initiate payouts. Chairperson role required.", 
                userId, member.Role);
        }

        return canInitiate;
    }

    public async Task<bool> CheckGroupEditPermissionAsync(string userId, Guid groupId)
    {
        // Group editing allowed for Chairperson and Treasurer
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        if (member == null)
        {
            _logger.LogWarning("User {UserId} is not a member of group {GroupId}", userId, groupId);
            return false;
        }

        var canEdit = member.Role == MemberRole.Chairperson || member.Role == MemberRole.Treasurer;

        if (!canEdit)
        {
            _logger.LogWarning("Authorization failed: User {UserId} with role {Role} cannot edit group settings", 
                userId, member.Role);
        }

        return canEdit;
    }

    public async Task<bool> CheckViewFullMemberRosterPermissionAsync(string userId, Guid groupId)
    {
        // Full member roster (with unmasked PII) visible only to Chairperson (SP-06)
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        if (member == null)
        {
            return false;
        }

        return member.Role == MemberRole.Chairperson;
    }

    public async Task<MemberRole?> GetUserRoleInGroupAsync(string userId, Guid groupId)
    {
        var member = await _memberRepository.GetByGroupIdAndUserIdAsync(groupId, userId);
        return member?.Role;
    }
}
