using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for interest calculation and capitalization (GW-03, GW-04)
/// </summary>
public interface IInterestService
{
    Task<Result<decimal>> CalculateDailyInterestAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Result> CapitalizeMonthlyInterestAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<Result> ProcessAllGroupsInterestAsync(CancellationToken cancellationToken = default);
    decimal CalculateInterestRate(decimal balance);
}
