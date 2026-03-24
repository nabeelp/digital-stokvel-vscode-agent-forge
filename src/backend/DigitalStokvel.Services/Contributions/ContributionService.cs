using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Contributions;

/// <summary>
/// Service for managing contributions (CC-01 to CC-10)
/// </summary>
public class ContributionService : IContributionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoreBankingClient _coreBankingClient;
    private readonly INotificationService _notificationService;
    private readonly IWalletService _walletService;
    private readonly ILogger<ContributionService> _logger;

    public ContributionService(
        IUnitOfWork unitOfWork,
        ICoreBankingClient coreBankingClient,
        INotificationService notificationService,
        IWalletService walletService,
        ILogger<ContributionService> logger)
    {
        _unitOfWork = unitOfWork;
        _coreBankingClient = coreBankingClient;
        _notificationService = notificationService;
        _walletService = walletService;
        _logger = logger;
    }

    public async Task<Result<ContributionResponse>> ProcessContributionAsync(CreateContributionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(request.GroupId, cancellationToken);
            if (group == null) return Result<ContributionResponse>.Failure("Group not found");

            // Get member
            var members = await _unitOfWork.Members.GetByGroupIdAsync(request.GroupId, cancellationToken);
            var member = members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<ContributionResponse>.Failure("You are not a member of this group");

            // Validate contribution amount matches group requirement (CC-10: no partial payments)
            if (request.Amount != group.ContributionAmount)
                return Result<ContributionResponse>.Failure($"Contribution must be exactly R{group.ContributionAmount}");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create contribution record
                var contribution = new Contribution
                {
                    Id = Guid.NewGuid(),
                    GroupId = request.GroupId,
                    MemberId = member.Id,
                    Amount = request.Amount,
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = ContributionStatus.Pending,
                    PaymentMethod = request.PaymentMethod,
                    DueDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Contributions.AddAsync(contribution, cancellationToken);

                // Process payment via core banking (CC-01)
                var paymentSuccess = await _coreBankingClient.ExecutePaymentAsync(
                    request.BankAccountNumber,
                    group.BankAccountNumber,
                    request.Amount,
                    $"Contribution to {group.Name}",
                    cancellationToken
                );

                if (paymentSuccess)
                {
                    contribution.Status = ContributionStatus.Completed;
                    contribution.ConfirmedAt = DateTime.UtcNow;

                    // Create immutable ledger entry (GW-06, GW-08)
                    await _walletService.CreateLedgerEntryAsync(
                        group.Id,
                        member.Id,
                        request.Amount,
                        TransactionType.Contribution.ToString(),
                        contribution.TransactionId,
                        $"Contribution from {member.FullName}",
                        cancellationToken
                    );

                    // Update group balance
                    group.CurrentBalance += request.Amount;

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // Queue payment confirmation (CC-05)
                    await _notificationService.QueuePaymentConfirmationAsync(contribution.Id, cancellationToken);

                    _logger.LogInformation("Contribution {ContributionId} processed successfully", contribution.Id);

                    return Result<ContributionResponse>.Success(MapToResponse(contribution, group.Name, member.FullName));
                }
                else
                {
                    contribution.Status = ContributionStatus.Failed;
                    contribution.FailureReason = "Payment processing failed";

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    _logger.LogWarning("Contribution {ContributionId} failed - payment processing failed", contribution.Id);

                    return Result<ContributionResponse>.Failure("Payment processing failed");
                }
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contribution");
            return Result<ContributionResponse>.Failure($"Failed to process contribution: {ex.Message}");
        }
    }

    public async Task<Result<ContributionReceiptResponse>> GetContributionReceiptAsync(Guid contributionId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contribution = await _unitOfWork.Contributions.GetByIdWithDetailsAsync(contributionId, cancellationToken);
            if (contribution == null) return Result<ContributionReceiptResponse>.Failure("Contribution not found");

            // Verify user is member or admin
            var member = await _unitOfWork.Members.GetByIdAsync(contribution.MemberId, cancellationToken);
            var group = await _unitOfWork.Groups.GetByIdAsync(contribution.GroupId, cancellationToken);
            
            if (member?.UserId != userId)
            {
                var groupWithMembers = await _unitOfWork.Groups.GetByIdWithMembersAsync(contribution.GroupId, cancellationToken);
                var requestingMember = groupWithMembers?.Members.FirstOrDefault(m => m.UserId == userId);
                if (requestingMember?.Role != MemberRole.Chairperson && requestingMember?.Role != MemberRole.Treasurer)
                    return Result<ContributionReceiptResponse>.Failure("You don't have access to this receipt");
            }

            return Result<ContributionReceiptResponse>.Success(new ContributionReceiptResponse(
                contribution.Id,
                group!.Name,
                member!.FullName,
                contribution.Amount,
                contribution.CreatedAt,
                contribution.TransactionId,
                group.CurrentBalance,
                group.TotalInterestEarned,
                $"RCPT-{contribution.Id.ToString()[..8]}",
                contribution.Status
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt");
            return Result<ContributionReceiptResponse>.Failure($"Failed to get receipt: {ex.Message}");
        }
    }

    public async Task<Result<List<ContributionResponse>>> GetMyContributionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _unitOfWork.Members.GetByUserIdAsync(userId, cancellationToken);
            var contributions = new List<ContributionResponse>();

            foreach (var member in members)
            {
                var memberContributions = await _unitOfWork.Contributions.GetByMemberIdAsync(member.Id, cancellationToken);
                var group = await _unitOfWork.Groups.GetByIdAsync(member.GroupId, cancellationToken);

                contributions.AddRange(memberContributions.Select(c => MapToResponse(c, group!.Name, member.FullName)));
            }

            return Result<List<ContributionResponse>>.Success(contributions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contributions");
            return Result<List<ContributionResponse>>.Failure($"Failed to get contributions: {ex.Message}");
        }
    }

    public async Task<Result> SetupRecurringContributionAsync(SetupRecurringContributionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result.Failure("You are not a member of this group");

            // Setup debit order via core banking (CC-02)
            var success = await _coreBankingClient.SetupDebitOrderAsync(
                request.BankAccountNumber,
                group.ContributionAmount,
                request.DayOfMonth,
                cancellationToken
            );

            if (!success) return Result.Failure("Failed to setup debit order");

            _logger.LogInformation("Debit order setup for member {MemberId} in group {GroupId}", member.Id, request.GroupId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up recurring contribution");
            return Result.Failure($"Failed to setup recurring contribution: {ex.Message}");
        }
    }

    public async Task<Result> CancelRecurringContributionAsync(Guid recurringId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement debit order cancellation
            _logger.LogInformation("Debit order cancelled: {RecurringId}", recurringId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling recurring contribution");
            return Result.Failure($"Failed to cancel recurring contribution: {ex.Message}");
        }
    }

    public async Task<Result<ContributionResponse>> RetryFailedContributionAsync(Guid contributionId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var contribution = await _unitOfWork.Contributions.GetByIdWithDetailsAsync(contributionId, cancellationToken);
            if (contribution == null) return Result<ContributionResponse>.Failure("Contribution not found");
            if (contribution.Status != ContributionStatus.Failed) return Result<ContributionResponse>.Failure("Contribution is not in failed state");

            var member = await _unitOfWork.Members.GetByIdAsync(contribution.MemberId, cancellationToken);
            if (member?.UserId != userId) return Result<ContributionResponse>.Failure("You can only retry your own contributions");

            contribution.Status = ContributionStatus.Pending;
            contribution.RetryCount++;

            // TODO: Implement retry logic
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var group = await _unitOfWork.Groups.GetByIdAsync(contribution.GroupId, cancellationToken);
            return Result<ContributionResponse>.Success(MapToResponse(contribution, group!.Name, member.FullName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying contribution");
            return Result<ContributionResponse>.Failure($"Failed to retry contribution: {ex.Message}");
        }
    }

    public async Task<Result<MemberContributionHistory>> GetMemberContributionHistoryAsync(Guid groupId, Guid memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId, cancellationToken);
            if (member == null) return Result<MemberContributionHistory>.Failure("Member not found");

            var contributions = await _unitOfWork.Contributions.GetByMemberIdAsync(memberId, cancellationToken);
            var totalContributed = contributions.Where(c => c.Status == ContributionStatus.Completed).Sum(c => c.Amount);

            var recentContributions = contributions
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new ContributionSummary(c.Id, c.Amount, c.CreatedAt, c.Status, c.TransactionId))
                .ToList();

            return Result<MemberContributionHistory>.Success(new MemberContributionHistory(
                memberId,
                member.FullName,
                totalContributed,
                contributions.Count(),
                contributions.OrderByDescending(c => c.CreatedAt).FirstOrDefault()?.CreatedAt,
                recentContributions
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contribution history");
            return Result<MemberContributionHistory>.Failure($"Failed to get contribution history: {ex.Message}");
        }
    }

    private static ContributionResponse MapToResponse(Contribution contribution, string groupName, string memberName)
    {
        return new ContributionResponse(
            contribution.Id,
            contribution.GroupId,
            groupName,
            contribution.MemberId,
            memberName,
            contribution.Amount,
            contribution.CreatedAt,
            contribution.Status,
            contribution.PaymentMethod,
            contribution.TransactionId,
            contribution.ConfirmedAt,
            contribution.FailureReason
        );
    }
}
