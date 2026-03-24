using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Groups;

/// <summary>
/// Service for managing stokvel groups (GM-01 to GM-09)
/// </summary>
public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoreBankingClient _coreBankingClient;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        IUnitOfWork unitOfWork,
        ICoreBankingClient coreBankingClient,
        INotificationService notificationService,
        ILogger<GroupService> logger)
    {
        _unitOfWork = unitOfWork;
        _coreBankingClient = coreBankingClient;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<GroupResponse>> CreateGroupAsync(CreateGroupRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate contribution amount (GM-02: min R50, max R10,000)
            if (request.ContributionAmount < 50 || request.ContributionAmount > 10000)
            {
                return Result<GroupResponse>.Failure("Contribution amount must be between R50 and R10,000");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create group savings account via core banking (GM-06)
                var bankAccountNumber = await _coreBankingClient.CreateGroupSavingsAccountAsync(request.Name, userId, cancellationToken);

                // Create group entity
                var group = new Group
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    GroupType = request.GroupType,
                    ContributionAmount = request.ContributionAmount,
                    ContributionFrequency = request.ContributionFrequency,
                    PayoutSchedule = request.PayoutSchedule,
                    CurrentBalance = 0,
                    TotalInterestEarned = 0,
                    Status = GroupStatus.Active,
                    BankAccountNumber = bankAccountNumber,
                    ChairpersonId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Groups.AddAsync(group, cancellationToken);

                // Create chairperson member
                var chairperson = new Member
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    UserId = userId,
                    FullName = "Chairperson",
                    PhoneNumber = "",
                    Role = MemberRole.Chairperson,
                    Status = MemberStatus.Active,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Members.AddAsync(chairperson, cancellationToken);
                group.ChairpersonId = chairperson.Id;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Group {GroupName} created by user {UserId}", request.Name, userId);

                return Result<GroupResponse>.Success(new GroupResponse(
                    group.Id, group.Name, group.Description, group.GroupType,
                    group.ContributionAmount, group.ContributionFrequency,
                    group.PayoutSchedule, group.CurrentBalance, group.TotalInterestEarned,
                    group.Status, 1, group.CreatedAt
                ));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return Result<GroupResponse>.Failure($"Failed to create group: {ex.Message}");
        }
    }

    public async Task<Result<GroupDetailsResponse>> GetGroupDetailsAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result<GroupDetailsResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<GroupDetailsResponse>.Failure("You are not a member of this group");

            var memberSummaries = new List<MemberSummary>();
            foreach (var m in group.Members)
            {
                var totalContributed = await _unitOfWork.Contributions.GetTotalContributedAsync(m.Id, cancellationToken);
                memberSummaries.Add(new MemberSummary(
                    m.Id, m.FullName, m.PhoneNumber, m.Role, m.Status,
                    totalContributed, m.JoinedAt, "Current"
                ));
            }

            var totalContributions = group.Contributions.Where(c => c.Status == ContributionStatus.Completed).Sum(c => c.Amount);
            var totalPayouts = group.Payouts.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.Amount);

            var stats = new GroupStats(totalContributions, totalPayouts, group.TotalInterestEarned,
                group.Members.Count, group.Members.Count(m => m.Status == MemberStatus.Active), null);

            return Result<GroupDetailsResponse>.Success(new GroupDetailsResponse(
                group.Id, group.Name, group.Description, group.GroupType,
                group.ContributionAmount, group.ContributionFrequency, group.PayoutSchedule,
                group.CurrentBalance, group.TotalInterestEarned, group.Status,
                group.CreatedAt, memberSummaries, stats
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group details");
            return Result<GroupDetailsResponse>.Failure($"Failed to get group details: {ex.Message}");
        }
    }

    public async Task<Result<List<GroupResponse>>> GetMyGroupsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByUserIdAsync(userId, cancellationToken);
            var groups = new List<GroupResponse>();

            foreach (var member in members)
            {
                var group = await _unitOfWork.Groups.GetByIdAsync(member.GroupId, cancellationToken);
                if (group != null)
                {
                    var memberCount = await _unitOfWork.Members.GetGroupMemberCountAsync(group.Id, cancellationToken);
                    groups.Add(new GroupResponse(
                        group.Id, group.Name, group.Description, group.GroupType,
                        group.ContributionAmount, group.ContributionFrequency, group.PayoutSchedule,
                        group.CurrentBalance, group.TotalInterestEarned, group.Status,
                        memberCount, group.CreatedAt
                    ));
                }
            }

            return Result<List<GroupResponse>>.Success(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups");
            return Result<List<GroupResponse>>.Failure($"Failed to get groups: {ex.Message}");
        }
    }

    public async Task<Result> UpdateGroupAsync(Guid groupId, UpdateGroupRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || (member.Role != MemberRole.Treasurer && member.Role != MemberRole.Chairperson))
                return Result.Failure("Only Treasurer or Chairperson can update group details");

            if (!string.IsNullOrWhiteSpace(request.Description))
                group.Description = request.Description;

            if (request.ContributionAmount.HasValue)
            {
                if (request.ContributionAmount.Value < 50 || request.ContributionAmount.Value > 10000)
                    return Result.Failure("Contribution amount must be between R50 and R10,000");
                group.ContributionAmount = request.ContributionAmount.Value;
            }

            if (request.ContributionFrequency.HasValue)
                group.ContributionFrequency = request.ContributionFrequency.Value;

            group.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Group {GroupId} updated by user {UserId}", groupId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group");
            return Result.Failure($"Failed to update group: {ex.Message}");
        }
    }

    public async Task<Result> ArchiveGroupAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || member.Role != MemberRole.Chairperson)
                return Result.Failure("Only Chairperson can archive group");

            group.Status = GroupStatus.Archived;
            group.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Group {GroupId} archived by user {UserId}", groupId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving group");
            return Result.Failure($"Failed to archive group: {ex.Message}");
        }
    }

    public async Task<Result> AddMemberAsync(Guid groupId, InviteMemberRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            var inviter = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (inviter == null || inviter.Role != MemberRole.Chairperson)
                return Result.Failure("Only Chairperson can invite members");

            if (group.Members.Any(m => m.PhoneNumber == request.PhoneNumber))
                return Result.Failure("Member already exists");

            if (request.Role == MemberRole.Treasurer && group.Members.Any(m => m.Role == MemberRole.Treasurer))
                return Result.Failure("Group already has a Treasurer");

            var member = new Member
            {
                Id = Guid.NewGuid(), GroupId = groupId, UserId = Guid.NewGuid().ToString(),
                PhoneNumber = request.PhoneNumber, FullName = request.FullName, Role = request.Role,
                Status = MemberStatus.Invited, InvitedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Members.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notificationService.QueueInvitationNotificationAsync(member.Id, groupId, cancellationToken);

            _logger.LogInformation("Member invited to group {GroupId}", groupId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting member");
            return Result.Failure($"Failed to invite member: {ex.Message}");
        }
    }

    public async Task<Result> RemoveMemberAsync(Guid groupId, Guid memberId, RemoveMemberRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            var requestingMember = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (requestingMember == null || (requestingMember.Role != MemberRole.Chairperson && requestingMember.Role != MemberRole.Treasurer))
                return Result.Failure("Only Chairperson or Treasurer can remove members");

            var memberToRemove = group.Members.FirstOrDefault(m => m.Id == memberId);
            if (memberToRemove == null) return Result.Failure("Member not found");
            if (memberToRemove.Role == MemberRole.Chairperson) return Result.Failure("Cannot remove Chairperson");

            memberToRemove.Status = MemberStatus.Removed;
            memberToRemove.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} removed from group {GroupId}", memberId, groupId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member");
            return Result.Failure($"Failed to remove member: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalculateGroupBalanceAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return Result<decimal>.Failure("Group not found");

            var totalContributions = await _unitOfWork.Contributions.GetTotalByGroupAsync(groupId, ContributionStatus.Completed, cancellationToken);
            var totalPayouts = await _unitOfWork.Payouts.GetTotalByGroupAsync(groupId, PayoutStatus.Completed, cancellationToken);
            var balance = totalContributions - totalPayouts + group.TotalInterestEarned;

            return Result<decimal>.Success(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating balance");
            return Result<decimal>.Failure($"Failed to calculate balance: {ex.Message}");
        }
    }
}
