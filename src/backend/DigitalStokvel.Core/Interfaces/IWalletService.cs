using DigitalStokvel.Core.DTOs;

namespace DigitalStokvel.Core.Interfaces;

/// <summary>
/// Service for wallet and ledger operations (GW-01 to GW-09)
/// </summary>
public interface IWalletService
{
    Task<Result<GroupBalanceResponse>> GetGroupBalanceAsync(Guid groupId, string userId, CancellationToken cancellationToken = default);
    Task<Result<PagedLedgerResponse>> GetLedgerEntriesAsync(Guid groupId, PaginationRequest pagination, string userId, CancellationToken cancellationToken = default);
    Task<Result> CreateLedgerEntryAsync(Guid groupId, Guid? memberId, decimal amount, string transactionType, string transactionId, string description, CancellationToken cancellationToken = default);
}
