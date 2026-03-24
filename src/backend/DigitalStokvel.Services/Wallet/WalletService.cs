using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Wallet;

public class WalletService : IWalletService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IUnitOfWork unitOfWork, ILogger<WalletService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GroupBalanceResponse>> GetGroupBalanceAsync(Guid groupId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result<GroupBalanceResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<GroupBalanceResponse>.Failure("You are not a member of this group");

            var totalContributions = group.Contributions.Where(c => c.Status == ContributionStatus.Completed).Sum(c => c.Amount);
            var totalPayouts = group.Payouts.Where(p => p.Status == PayoutStatus.Completed).Sum(p => p.Amount);

            // Calculate YTD interest
            var ytdInterest = group.InterestTransactions
                .Where(it => it.CreatedAt.Year == DateTime.UtcNow.Year)
                .Sum(it => it.InterestAmount);

            return Result<GroupBalanceResponse>.Success(new GroupBalanceResponse(
                groupId,
                group.Name,
                group.CurrentBalance,
                totalContributions,
                totalPayouts,
                group.TotalInterestEarned,
                ytdInterest,
                DateTime.UtcNow,
                null // TODO: Calculate next payout date
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group balance");
            return Result<GroupBalanceResponse>.Failure($"Failed to get balance: {ex.Message}");
        }
    }

    public async Task<Result<PagedLedgerResponse>> GetLedgerEntriesAsync(Guid groupId, PaginationRequest pagination, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId, cancellationToken);
            if (group == null) return Result<PagedLedgerResponse>.Failure("Group not found");

            var member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return Result<PagedLedgerResponse>.Failure("You are not a member of this group");

            var ledgerEntries = await _unitOfWork.Ledger.GetByGroupIdPagedAsync(groupId, pagination.PageNumber, pagination.PageSize, cancellationToken);
            var totalCount = await _unitOfWork.Ledger.GetCountByGroupIdAsync(groupId, cancellationToken);

            var entries = ledgerEntries.Select(le =>
            {
                var entryMember = le.MemberId.HasValue ? group.Members.FirstOrDefault(m => m.Id == le.MemberId) : null;
                return new LedgerEntryResponse(
                    le.Id,
                    le.Date,
                    le.MemberId,
                    entryMember?.FullName,
                    le.TransactionType,
                    le.Amount,
                    le.BalanceAfter,
                    le.TransactionId,
                    le.Description
                );
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize);

            return Result<PagedLedgerResponse>.Success(new PagedLedgerResponse(
                entries,
                pagination.PageNumber,
                pagination.PageSize,
                totalCount,
                totalPages
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ledger entries");
            return Result<PagedLedgerResponse>.Failure($"Failed to get ledger: {ex.Message}");
        }
    }

    public async Task<Result> CreateLedgerEntryAsync(Guid groupId, Guid? memberId, decimal amount, string transactionType, string transactionId, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            // Calculate balance after transaction
            var balanceAfter = group.CurrentBalance;

            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                MemberId = memberId,
                Date = DateTime.UtcNow,
                TransactionType = Enum.Parse<TransactionType>(transactionType),
                Amount = amount,
                BalanceAfter = balanceAfter,
                TransactionId = transactionId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Ledger.AddAsync(ledgerEntry, cancellationToken);

            _logger.LogInformation("Ledger entry created for group {GroupId}: {TransactionType} {Amount}", groupId, transactionType, amount);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ledger entry");
            return Result.Failure($"Failed to create ledger entry: {ex.Message}");
        }
    }
}
