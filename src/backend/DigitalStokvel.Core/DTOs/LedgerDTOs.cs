using DigitalStokvel.Core.Enums;

namespace DigitalStokvel.Core.DTOs;

/// <summary>
/// Ledger entry response (GW-05, GW-08)
/// </summary>
public record LedgerEntryResponse(
    Guid Id,
    DateTime Date,
    Guid? MemberId,
    string? MemberName,
    TransactionType TransactionType,
    decimal Amount,
    decimal BalanceAfter,
    string TransactionId,
    string Description
);

/// <summary>
/// Paginated ledger response
/// </summary>
public record PagedLedgerResponse(
    List<LedgerEntryResponse> Entries,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);

/// <summary>
/// Group balance response (GW-02)
/// </summary>
public record GroupBalanceResponse(
    Guid GroupId,
    string GroupName,
    decimal CurrentBalance,
    decimal TotalContributions,
    decimal TotalPayouts,
    decimal TotalInterestEarned,
    decimal InterestEarnedYTD,
    DateTime LastUpdated,
    DateTime? NextPayoutDate
);
