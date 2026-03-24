namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Authorization service interface for RBAC (SP-03)
/// </summary>
public interface IAuthorizationService
{
    Task<bool> CheckGroupMembershipAsync(string userId, Guid groupId);
    Task<bool> CheckRoleAsync(string userId, Guid groupId, Enums.MemberRole requiredRole);
    Task<bool> CheckPayoutApprovalPermissionAsync(string userId, Guid payoutId);
    Task<bool> CheckPayoutInitiationPermissionAsync(string userId, Guid groupId);
    Task<bool> CheckGroupEditPermissionAsync(string userId, Guid groupId);
    Task<bool> CheckViewFullMemberRosterPermissionAsync(string userId, Guid groupId);
    Task<Enums.MemberRole?> GetUserRoleInGroupAsync(string userId, Guid groupId);
}
