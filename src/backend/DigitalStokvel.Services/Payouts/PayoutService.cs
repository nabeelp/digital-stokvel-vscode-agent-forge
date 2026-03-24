using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Payouts;

/// <summary>
/// Service for managing payouts (PE-01 to PE-09)
/// </summary>
public class PayoutService : IPayoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoreBankingClient _coreBankingClient;
    private readonly INotificationService _notificationService;
    private readonly IWalletService _walletService;
    private readonly ILogger<PayoutService> _logger;

    public PayoutService(
        IUnitOfWork unitOfWork,
        ICoreBankingClient coreBankingClient,
        INotificationService notificationService,
        IWalletService walletService,
        ILogger<PayoutService> logger)
    {
        _unitOfWork = unitOfWork;
        _coreBankingClient = coreBankingClient;
        _notificationService = notificationService;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<Result<PayoutResponse>> InitiatePayoutAsync(InitiatePayoutRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null) return Result<PayoutResponse>.Failure("Group not found");

            // Verify user is Chairperson (PE-02)
            var initiator = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (initiator == null || initiator.Role != MemberRole.Chairperson)
                return Result<PayoutResponse>.Failure("Only Chairperson can initiate payouts");

            // Verify sufficient balance (PE-02)
            if (group.CurrentBalance < request.Amount)
                return Result<PayoutResponse>.Failure("Insufficient group balance");

            // Verify recipient is a member
            var recipient = group.Members.FirstOrDefault(m => m.Id == request.RecipientMemberId);
            if (recipient == null) return Result<PayoutResponse>.Failure("Recipient is not a member of this group");

            // Create payout record
            var payout = new Payout
            {
                Id = Guid.NewGuid(),
                GroupId = request.GroupId,
                RecipientMemberId = request.RecipientMemberId,
                Amount = request.Amount,
                PayoutType = request.PayoutType,
                Status = PayoutStatus.PendingApproval,
                InitiatedByMemberId = initiator.Id,
                InitiatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // PE-03: 24-hour expiration
                Notes = request.Notes,
                TransactionId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payouts.AddAsync(payout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payout {PayoutId} initiated by {InitiatorId} for recipient {RecipientId}", 
                payout.Id, initiator.Id, request.RecipientMemberId);

            return Result<PayoutResponse>.Success(MapToResponse(payout, group.Name, recipient.FullName, initiator.FullName, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payout");
            return Result<PayoutResponse>.Failure($"Failed to initiate payout: {ex.Message}");
        }
    }

    public async Task<Result<PayoutResponse>> ApprovePayoutAsync(Guid payoutId, ApprovePayoutRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payout = await _unitOfWork.Payouts.GetByIdWithDetailsAsync(payoutId, cancellationToken);
            if (payout == null) return Result<PayoutResponse>.Failure("Payout not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(payout.GroupId, cancellationToken);
            if (group == null) return Result<PayoutResponse>.Failure("Group not found");

            // Verify user is Treasurer (PE-03)
            var approver = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (approver == null || approver.Role != MemberRole.Treasurer)
                return Result<PayoutResponse>.Failure("Only Treasurer can approve payouts");

            // Check if payout has expired (PE-03: 24-hour expiration)
            if (payout.ExpiresAt.HasValue && DateTime.UtcNow > payout.ExpiresAt.Value)
            {
                payout.Status = PayoutStatus.Expired;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<PayoutResponse>.Failure("Payout has expired. Please initiate a new payout.");
            }

            if (payout.Status != PayoutStatus.PendingApproval)
                return Result<PayoutResponse>.Failure($"Payout is not pending approval (current status: {payout.Status})");

            payout.Status = PayoutStatus.Approved;
            payout.ApprovedByMemberId = approver.Id;
            payout.ApprovedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Execute payout immediately (PE-04)
            var executeResult = await ExecutePayoutAsync(payoutId, cancellationToken);
            if (!executeResult.IsSuccess)
            {
                payout.Status = PayoutStatus.Failed;
                payout.FailureReason = executeResult.ErrorMessage;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<PayoutResponse>.Failure(executeResult.ErrorMessage ?? "Payout execution failed");
            }

            var recipient = group.Members.First(m => m.Id == payout.RecipientMemberId);
            var initiator = group.Members.First(m => m.Id == payout.InitiatedByMemberId);

            _logger.LogInformation("Payout {PayoutId} approved and executed", payoutId);

            return Result<PayoutResponse>.Success(MapToResponse(payout, group.Name, recipient.FullName, 
                initiator.FullName, approver.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving payout");
            return Result<PayoutResponse>.Failure($"Failed to approve payout: {ex.Message}");
        }
    }

    public async Task<Result> RejectPayoutAsync(Guid payoutId, RejectPayoutRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payout = await _unitOfWork.Payouts.GetByIdWithDetailsAsync(payoutId, cancellationToken);
            if (payout == null) return Result.Failure("Payout not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(payout.GroupId, cancellationToken);
            var member = group?.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || member.Role != MemberRole.Treasurer)
                return Result.Failure("Only Treasurer can reject payouts");

            payout.Status = PayoutStatus.Rejected;
            payout.FailureReason = request.Reason;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payout {PayoutId} rejected by {MemberId}: {Reason}", payoutId, member.Id, request.Reason);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting payout");
            return Result.Failure($"Failed to reject payout: {ex.Message}");
        }
    }

    public async Task<Result<PayoutResponse>> GetPayoutDetailsAsync(Guid payoutId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payout = await _unitOfWork.Payouts.GetByIdWithDetailsAsync(payoutId, cancellationToken);
            if (payout == null) return Result<PayoutResponse>.Failure("Payout not found");

            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(payout.GroupId, cancellationToken);
            if (group == null) return Result<PayoutResponse>.Failure("Group not found");
            
            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<PayoutResponse>.Failure("You are not a member of this group");

            var recipient = group.Members.First(m => m.Id == payout.RecipientMemberId);
            var initiator = group.Members.First(m => m.Id == payout.InitiatedByMemberId);
            var approver = payout.ApprovedByMemberId.HasValue ? 
                group.Members.FirstOrDefault(m => m.Id == payout.ApprovedByMemberId.Value) : null;

            return Result<PayoutResponse>.Success(MapToResponse(payout, group!.Name, recipient.FullName, 
                initiator.FullName, approver?.FullName ?? "Pending"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payout details");
            return Result<PayoutResponse>.Failure($"Failed to get payout details: {ex.Message}");
        }
    }

    public async Task<Result<List<PayoutHistoryItem>>> GetGroupPayoutHistoryAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result<List<PayoutHistoryItem>>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<List<PayoutHistoryItem>>.Failure("You are not a member of this group");

            var payouts = await _unitOfWork.Payouts.GetByGroupIdAsync(groupId, cancellationToken);

            var history = payouts.Select(p =>
            {
                var recipient = group.Members.First(m => m.Id == p.RecipientMemberId);
                var initiator = group.Members.First(m => m.Id == p.InitiatedByMemberId);
                var approver = p.ApprovedByMemberId.HasValue ?
                    group.Members.FirstOrDefault(m => m.Id == p.ApprovedByMemberId.Value) : null;

                return new PayoutHistoryItem(
                    p.Id, p.RecipientMemberId, recipient.FullName, p.Amount, 0, // TODO: Calculate interest included
                    p.PayoutType, p.Status, p.InitiatedAt, initiator.FullName, approver?.FullName, p.CompletedAt
                );
            }).ToList();

            return Result<List<PayoutHistoryItem>>.Success(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payout history");
            return Result<List<PayoutHistoryItem>>.Failure($"Failed to get payout history: {ex.Message}");
        }
    }

    public async Task<Result<List<PayoutResponse>>> GetPendingApprovalsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByUserIdAsync(userId, cancellationToken);
            var treasurerMemberships = members.Where(m => m.Role == MemberRole.Treasurer).ToList();

            var pendingPayouts = new List<PayoutResponse>();

            foreach (var treasurerMember in treasurerMemberships)
            {
                var groupPayouts = await _unitOfWork.Payouts.GetByGroupIdAsync(treasurerMember.GroupId, cancellationToken);
                var pending = groupPayouts.Where(p => p.Status == PayoutStatus.PendingApproval);

                var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(treasurerMember.GroupId, cancellationToken);

                foreach (var payout in pending)
                {
                    var recipient = group!.Members.First(m => m.Id == payout.RecipientMemberId);
                    var initiator = group.Members.First(m => m.Id == payout.InitiatedByMemberId);

                    pendingPayouts.Add(MapToResponse(payout, group.Name, recipient.FullName, initiator.FullName, null));
                }
            }

            return Result<List<PayoutResponse>>.Success(pendingPayouts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return Result<List<PayoutResponse>>.Failure($"Failed to get pending approvals: {ex.Message}");
        }
    }

    public async Task<Result<NextPayoutRecipientResponse>> CalculateNextPayoutRecipientAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            // PE-01: Calculate next recipient in rotation
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result<NextPayoutRecipientResponse>.Failure("Group not found");

            // Get all payouts to determine next recipient
            var payouts = await _unitOfWork.Payouts.GetByGroupIdAsync(groupId, cancellationToken);
            var completedPayouts = payouts.Where(p => p.Status == PayoutStatus.Completed).OrderBy(p => p.CompletedAt).ToList();

            // Get active members
            var activeMembers = group.Members.Where(m => m.Status == MemberStatus.Active).OrderBy(m => m.FullName).ToList();

            if (!activeMembers.Any())
                return Result<NextPayoutRecipientResponse>.Failure("No active members");

            // Simple alphabetical rotation
            Guid nextRecipientId;
            DateTime lastPayoutDate = DateTime.MinValue;

            if (!completedPayouts.Any())
            {
                // First payout goes to first member alphabetically
                nextRecipientId = activeMembers.First().Id;
            }
            else
            {
                var lastPayout = completedPayouts.Last();
                lastPayoutDate = lastPayout.CompletedAt ?? lastPayout.InitiatedAt;
                var lastRecipientIndex = activeMembers.FindIndex(m => m.Id == lastPayout.RecipientMemberId);

                if (lastRecipientIndex == -1 || lastRecipientIndex == activeMembers.Count - 1)
                {
                    // Start from beginning
                    nextRecipientId = activeMembers.First().Id;
                }
                else
                {
                    nextRecipientId = activeMembers[lastRecipientIndex + 1].Id;
                }
            }

            var nextRecipient = activeMembers.First(m => m.Id == nextRecipientId);

            return Result<NextPayoutRecipientResponse>.Success(new NextPayoutRecipientResponse(
                nextRecipient.Id,
                nextRecipient.FullName,
                "Next in alphabetical rotation",
                lastPayoutDate
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next recipient");
            return Result<NextPayoutRecipientResponse>.Failure($"Failed to calculate next recipient: {ex.Message}");
        }
    }

    public async Task<Result> ExecutePayoutAsync(Guid payoutId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payout = await _unitOfWork.Payouts.GetByIdWithDetailsAsync(payoutId, cancellationToken);
            if (payout == null) return Result.Failure("Payout not found");

            var group = await _unitOfWork.Groups.GetByIdAsync(payout.GroupId, cancellationToken);
            var recipient = await _unitOfWork.Members.GetByIdAsync(payout.RecipientMemberId, cancellationToken);

            // Execute EFT via core banking (PE-04)
            var eftSuccess = await _coreBankingClient.ExecuteEFTAsync(
                recipient!.BankAccountNumber,
                payout.Amount,
                $"Stokvel payout from {group!.Name}",
                cancellationToken
            );

            if (eftSuccess)
            {
                payout.Status = PayoutStatus.Completed;
                payout.CompletedAt = DateTime.UtcNow;

                // Create ledger entry (GW-08)
                await _walletService.CreateLedgerEntryAsync(
                    group.Id,
                    recipient.Id,
                    -payout.Amount,
                    TransactionType.Payout.ToString(),
                    payout.TransactionId,
                    $"Payout to {recipient.FullName}",
                    cancellationToken
                );

                // Update group balance
                group.CurrentBalance -= payout.Amount;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Queue payout notification (PE-06)
                await _notificationService.QueuePayoutNotificationAsync(payoutId, cancellationToken);

                _logger.LogInformation("Payout {PayoutId} executed successfully", payoutId);
                return Result.Success();
            }
            else
            {
                payout.Status = PayoutStatus.Failed;
                payout.FailureReason = "EFT execution failed"; // PE-08
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Payout {PayoutId} failed: EFT execution failed", payoutId);
                return Result.Failure("EFT execution failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing payout");
            return Result.Failure($"Failed to execute payout: {ex.Message}");
        }
    }

    private static PayoutResponse MapToResponse(Payout payout, string groupName, string recipientName,
        string initiatorName, string? approverName)
    {
        return new PayoutResponse(
            payout.Id,
            payout.GroupId,
            groupName,
            payout.RecipientMemberId,
            recipientName,
            payout.Amount,
            0, // TODO: Calculate interest included
            payout.PayoutType,
            payout.Status,
            payout.InitiatedAt,
            payout.InitiatedByMemberId,
            initiatorName,
            payout.ApprovedByMemberId,
            approverName,
            payout.ApprovedAt,
            payout.CompletedAt,
            payout.ExpiresAt,
            payout.TransactionId,
            payout.FailureReason
        );
    }
}
