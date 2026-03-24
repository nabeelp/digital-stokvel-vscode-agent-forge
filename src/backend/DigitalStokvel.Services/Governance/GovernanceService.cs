using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Governance;

/// <summary>
/// Service for governance and disputes (GG-01 to GG-09)
/// </summary>
public class GovernanceService : IGovernanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<GovernanceService> _logger;

    public GovernanceService(IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<GovernanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<ConstitutionResponse>> CreateConstitutionAsync(CreateConstitutionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null) return Result<ConstitutionResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || member.Role != MemberRole.Chairperson)
                return Result<ConstitutionResponse>.Failure("Only Chairperson can create constitution");

            var constitution = new GroupConstitution
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                MissedPaymentPolicy = request.MissedPaymentPolicy,
                LateFeeAmount = request.LateFeeAmount ?? 0m,
                QuorumPercentage = request.QuorumPercentage,
                MemberRemovalProcess = request.MemberRemovalProcess,
                OtherRules = request.OtherRules,
                CreatedByMemberId = member.Id,
                CreatedAt = DateTime.UtcNow
            };

            // Assign constitution to group so EF Core tracks it
            group.Constitution = constitution;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<ConstitutionResponse>.Success(new ConstitutionResponse(
                constitution.Id, constitution.GroupId, constitution.MissedPaymentPolicy,
                constitution.LateFeeAmount, constitution.QuorumPercentage,
                constitution.MemberRemovalProcess, constitution.OtherRules,
                constitution.CreatedAt, constitution.CreatedByMemberId ?? Guid.Empty
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating constitution");
            return Result<ConstitutionResponse>.Failure($"Failed to create constitution: {ex.Message}");
        }
    }

    public async Task<Result<ConstitutionResponse>> GetConstitutionAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var constitution = await _unitOfWork.Groups.GetConstitutionByGroupIdAsync(groupId, cancellationToken);
            if (constitution == null) return Result<ConstitutionResponse>.Failure("Constitution not found");

            return Result<ConstitutionResponse>.Success(new ConstitutionResponse(
                constitution.Id, constitution.GroupId, constitution.MissedPaymentPolicy,
                constitution.LateFeeAmount, constitution.QuorumPercentage,
                constitution.MemberRemovalProcess, constitution.OtherRules,
                constitution.CreatedAt, constitution.CreatedByMemberId ?? Guid.Empty
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting constitution");
            return Result<ConstitutionResponse>.Failure($"Failed to get constitution: {ex.Message}");
        }
    }

    public async Task<Result<VoteResponse>> CreateVoteAsync(CreateVoteRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null) return Result<VoteResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || member.Role != MemberRole.Chairperson)
                return Result<VoteResponse>.Failure("Only Chairperson can create votes");

            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                Proposal = request.Proposal,
                Description = request.Description,
                Deadline = request.Deadline,
                Status = VoteStatus.Active,
                QuorumPercentage = request.QuorumPercentage,
                CreatedByMemberId = member.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Votes.AddAsync(vote, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var results = CalculateVoteResults(vote, group.Members.Count);

            return Result<VoteResponse>.Success(new VoteResponse(
                vote.Id, vote.GroupId, group.Name, vote.Proposal, vote.Description,
                vote.Deadline, vote.Status, vote.QuorumPercentage, results,
                vote.CreatedAt, vote.CreatedByMemberId, member.FullName, false
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vote");
            return Result<VoteResponse>.Failure($"Failed to create vote: {ex.Message}");
        }
    }

    public async Task<Result<VoteResponse>> CastVoteAsync(Guid voteId, CastVoteRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vote = await _unitOfWork.Votes.GetByIdWithRecordsAsync(voteId, cancellationToken);
            if (vote == null) return Result<VoteResponse>.Failure("Vote not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(vote.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<VoteResponse>.Failure("You are not a member of this group");

            if (vote.VoteRecords.Any(vr => vr.MemberId == member.Id))
                return Result<VoteResponse>.Failure("You have already voted");

            var voteRecord = new VoteRecord
            {
                Id = Guid.NewGuid(),
                VoteId = voteId,
                MemberId = member.Id,
                Choice = request.Choice,
                Comments = request.Comments,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Votes.AddRecordAsync(voteRecord, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var creator = group!.Members.First(m => m.Id == vote.CreatedByMemberId);
            var results = CalculateVoteResults(vote, group.Members.Count);

            return Result<VoteResponse>.Success(new VoteResponse(
                vote.Id, vote.GroupId, group.Name, vote.Proposal, vote.Description,
                vote.Deadline, vote.Status, vote.QuorumPercentage, results,
                vote.CreatedAt, vote.CreatedByMemberId, creator.FullName, true
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote");
            return Result<VoteResponse>.Failure($"Failed to cast vote: {ex.Message}");
        }
    }

    public async Task<Result<VoteResponse>> GetVoteDetailsAsync(Guid voteId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vote = await _unitOfWork.Votes.GetByIdWithRecordsAsync(voteId, cancellationToken);
            if (vote == null) return Result<VoteResponse>.Failure("Vote not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(vote.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<VoteResponse>.Failure("You are not a member of this group");

            var creator = group!.Members.First(m => m.Id == vote.CreatedByMemberId);
            var hasVoted = vote.VoteRecords.Any(vr => vr.MemberId == member.Id);
            var results = CalculateVoteResults(vote, group.Members.Count);

            return Result<VoteResponse>.Success(new VoteResponse(
                vote.Id, vote.GroupId, group.Name, vote.Proposal, vote.Description,
                vote.Deadline, vote.Status, vote.QuorumPercentage, results,
                vote.CreatedAt, creator.Id, creator.FullName, hasVoted
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vote details");
            return Result<VoteResponse>.Failure($"Failed to get vote: {ex.Message}");
        }
    }

    public async Task<Result<DisputeResponse>> RaiseDisputeAsync(RaiseDisputeRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null) return Result<DisputeResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<DisputeResponse>.Failure("You are not a member of this group");

            var dispute = new Dispute
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                RaisedByMemberId = member.Id,
                IssueType = request.IssueType,
                Description = request.Description,
                Status = DisputeStatus.Open,
                RaisedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Disputes.AddAsync(dispute, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notificationService.QueueDisputeNotificationAsync(dispute.Id, cancellationToken);

            return Result<DisputeResponse>.Success(new DisputeResponse(
                dispute.Id, dispute.GroupId, group.Name, member.Id, member.FullName,
                dispute.IssueType, dispute.Description, dispute.Status, dispute.RaisedAt,
                null, null, false, null, new List<DisputeMessageResponse>()
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising dispute");
            return Result<DisputeResponse>.Failure($"Failed to raise dispute: {ex.Message}");
        }
    }

    public async Task<Result<DisputeResponse>> GetDisputeDetailsAsync(Guid disputeId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dispute = await _unitOfWork.Disputes.GetByIdWithMessagesAsync(disputeId, cancellationToken);
            if (dispute == null) return Result<DisputeResponse>.Failure("Dispute not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(dispute.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<DisputeResponse>.Failure("You are not a member of this group");

            var raiser = group!.Members.First(m => m.Id == dispute.RaisedByMemberId);

            var messages = dispute.DisputeMessages.Select(dm =>
            {
                var messageMember = group.Members.First(m => m.Id == dm.MemberId);
                return new DisputeMessageResponse(dm.Id, dm.MemberId, messageMember.FullName, dm.Message, dm.CreatedAt);
            }).ToList();

            return Result<DisputeResponse>.Success(new DisputeResponse(
                dispute.Id, dispute.GroupId, group.Name, raiser.Id, raiser.FullName,
                dispute.IssueType, dispute.Description, dispute.Status, dispute.RaisedAt,
                dispute.ResolvedAt, dispute.Resolution, false, null, messages
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dispute");
            return Result<DisputeResponse>.Failure($"Failed to get dispute: {ex.Message}");
        }
    }

    public async Task<Result> AddDisputeMessageAsync(Guid disputeId, AddDisputeMessageRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dispute = await _unitOfWork.Disputes.GetByIdAsync(disputeId, cancellationToken);
            if (dispute == null) return Result.Failure("Dispute not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(dispute.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result.Failure("You are not a member of this group");

            var message = new DisputeMessage
            {
                Id = Guid.NewGuid(),
                DisputeId = disputeId,
                MemberId = member.Id,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Disputes.AddMessageAsync(message, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dispute message");
            return Result.Failure($"Failed to add message: {ex.Message}");
        }
    }

    public async Task<Result> ResolveDisputeAsync(Guid disputeId, ResolveDisputeRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dispute = await _unitOfWork.Disputes.GetByIdAsync(disputeId, cancellationToken);
            if (dispute == null) return Result.Failure("Dispute not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(dispute.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || (member.Role != MemberRole.Chairperson && member.Role != MemberRole.Treasurer))
                return Result.Failure("Only Chairperson or Treasurer can resolve disputes");

            dispute.Status = DisputeStatus.Resolved;
            dispute.Resolution = request.Resolution;
            dispute.ResolvedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving dispute");
            return Result.Failure($"Failed to resolve dispute: {ex.Message}");
        }
    }

    public async Task<Result> CheckAndEscalateDisputesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var openDisputes = await _unitOfWork.Disputes.GetOpenDisputesAsync(cancellationToken);
            var escalatedCount = 0;

            foreach (var dispute in openDisputes)
            {
                // GG-08: Auto-escalate after 7 days
                if ((DateTime.UtcNow - dispute.RaisedAt).TotalDays >= 7)
                {
                    dispute.Status = DisputeStatus.Escalated;
                    dispute.EscalationDeadline = DateTime.UtcNow;
                    escalatedCount++;
                }
            }

            if (escalatedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Escalated {Count} disputes", escalatedCount);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating disputes");
            return Result.Failure($"Failed to escalate disputes: {ex.Message}");
        }
    }

    private static VoteResults CalculateVoteResults(Vote vote, int totalMembers)
    {
        var yesCount = vote.VoteRecords.Count(vr => vr.Choice == VoteChoice.Yes);
        var noCount = vote.VoteRecords.Count(vr => vr.Choice == VoteChoice.No);
        var abstainCount = vote.VoteRecords.Count(vr => vr.Choice == VoteChoice.Abstain);
        var totalVotes = vote.VoteRecords.Count;
        var quorumRequired = (int)Math.Ceiling(totalMembers * (vote.QuorumPercentage / 100.0));
        var quorumMet = totalVotes >= quorumRequired;
        var outcome = quorumMet ? (yesCount > noCount ? "Passed" : "Failed") : "Pending";

        return new VoteResults(yesCount, noCount, abstainCount, totalVotes, totalMembers, quorumRequired, quorumMet, outcome);
    }
}
