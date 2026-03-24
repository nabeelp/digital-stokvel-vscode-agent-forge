using DigitalStokvel.Core.DTOs;
using DigitalStokvel.Core.Entities;
using DigitalStokvel.Core.Enums;
using DigitalStokvel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigitalStokvel.Services.Interest;

public class InterestService : IInterestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterestService> _logger;

    public InterestService(IUnitOfWork unitOfWork, ILogger<InterestService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<decimal>> CalculateDailyInterestAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return Result<decimal>.Failure("Group not found");

            var annualRate = CalculateInterestRate(group.CurrentBalance);
            var dailyRate = annualRate / 365;
            var interestAmount = group.CurrentBalance * dailyRate;

            return Result<decimal>.Success(interestAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating daily interest");
            return Result<decimal>.Failure($"Failed to calculate interest: {ex.Message}");
        }
    }

    public async Task<Result> CapitalizeMonthlyInterestAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId, cancellationToken);
            if (group == null) return Result.Failure("Group not found");

            // Calculate month's interest (simplified - should sum daily calculations)
            var annualRate = CalculateInterestRate(group.CurrentBalance);
            var monthlyRate = annualRate / 12;
            var interestAmount = group.CurrentBalance * monthlyRate;

            // Create interest transaction record
            var interestTransaction = new InterestTransaction
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                InterestAmount = interestAmount,
                Balance = group.CurrentBalance,
                InterestRate = annualRate,
                CalculatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            // Add to group's interest transactions collection
            group.InterestTransactions.Add(interestTransaction);

            // Update group balance
            group.CurrentBalance += interestAmount;
            group.TotalInterestEarned += interestAmount;

            // Create ledger entry
            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Date = DateTime.UtcNow,
                TransactionType = TransactionType.InterestCapitalization,
                Amount = interestAmount,
                BalanceAfter = group.CurrentBalance,
                TransactionId = Guid.NewGuid().ToString(),
                Description = $"Monthly interest capitalization at {annualRate:P2}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Ledger.AddAsync(ledgerEntry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Interest capitalized for group {GroupId}: {InterestAmount:C}", groupId, interestAmount);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capitalizing interest");
            return Result.Failure($"Failed to capitalize interest: {ex.Message}");
        }
    }

    public async Task<Result> ProcessAllGroupsInterestAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = await _unitOfWork.Groups.GetAllActiveAsync(cancellationToken);

            foreach (var group in groups)
            {
                await CapitalizeMonthlyInterestAsync(group.Id, cancellationToken);
            }

            _logger.LogInformation("Interest processed for {Count} groups", groups.Count());
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing all groups interest");
            return Result.Failure($"Failed to process interest: {ex.Message}");
        }
    }

    public decimal CalculateInterestRate(decimal balance)
    {
        // GW-04: Tiered interest schedule
        return balance switch
        {
            < 10000 => 0.035m,  // 3.5% for R0-R10K
            < 50000 => 0.045m,  // 4.5% for R10K-R50K
            _ => 0.055m         // 5.5% for R50K+
        };
    }
}
