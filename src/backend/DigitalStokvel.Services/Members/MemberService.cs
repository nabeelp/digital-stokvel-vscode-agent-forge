using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Members;

public class MemberService : IMemberService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MemberService> _logger;

    public MemberService(IUnitOfWork unitOfWork, ILogger<MemberService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<MemberResponse>> GetMemberProfileAsync(string userId,CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByUserIdAsync(userId, cancellationToken);
            var member = members.FirstOrDefault();
            if (member == null) return Result<MemberResponse>.Failure("Member not found");

            var group = await _unitOfWork.Groups.GetByIdAsync(member.GroupId, cancellationToken);

            return Result<MemberResponse>.Success(new MemberResponse(
                member.Id, member.GroupId, member.UserId, member.PhoneNumber,
                member.FullName, member.Role, member.Status, member.JoinedAt, group!.Name
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member profile");
            return Result<MemberResponse>.Failure($"Failed to get profile: {ex.Message}");
        }
    }

    public async Task<Result<MemberResponse>> GetMemberByIdAsync(Guid memberId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId, cancellationToken);
            if (member == null) return Result<MemberResponse>.Failure("Member not found");

            var group = await _unitOfWork.Groups.GetByIdAsync(member.GroupId, cancellationToken);

            return Result<MemberResponse>.Success(new MemberResponse(
                member.Id, member.GroupId, member.UserId, member.PhoneNumber,
                member.FullName, member.Role, member.Status, member.JoinedAt, group!.Name
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting member");
            return Result<MemberResponse>.Failure($"Failed to get member: {ex.Message}");
        }
    }

    public async Task<Result> UpdateMemberProfileAsync(UpdateMemberProfileRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByUserIdAsync(userId, cancellationToken);
            var member = members.FirstOrDefault();
            if (member == null) return Result.Failure("Member not found");

            if (!string.IsNullOrWhiteSpace(request.FullName))
                member.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                member.PhoneNumber = request.PhoneNumber;

            member.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return Result.Failure($"Failed to update profile: {ex.Message}");
        }
    }

    public async Task<Result> AcceptInvitationAsync(AcceptInvitationRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByGroupIdAsync(request.GroupId, cancellationToken);
            var member = members.FirstOrDefault(m => m.Status == MemberStatus.Invited && m.PhoneNumber != "");

            if (member == null) return Result.Failure("Invitation not found");

            member.UserId = userId;
            member.Status = MemberStatus.Active;
            member.JoinedAt = DateTime.UtcNow;
            member.BankAccountNumber = request.BankAccountNumber;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} accepted invitation to group {GroupId}", member.Id, request.GroupId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return Result.Failure($"Failed to accept invitation: {ex.Message}");
        }
    }
}
